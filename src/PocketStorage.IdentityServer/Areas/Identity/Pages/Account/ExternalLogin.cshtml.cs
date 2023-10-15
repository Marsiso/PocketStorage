using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using static System.String;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;

    public ExternalLoginModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IUserStore<User> userStore,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty] public ExternalLoginInput Form { get; set; } = default!;

    public string? ProviderDisplayName { get; set; }
    public string? ReturnUrl { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string? provider, string? returnUrl = null)
    {
        string? redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });

        AuthenticationProperties authenticationProperties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return new ChallengeResult(provider, authenticationProperties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = default, string? remoteError = default)
    {
        returnUrl ??= Url.Content("~/");
        if (!IsNullOrWhiteSpace(remoteError))
        {
            ErrorMessage = $"Error from external provider: {remoteError}";

            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        ExternalLoginInfo? externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo is null)
        {
            ErrorMessage = "Error loading external login information.";

            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, false, true);
        if (signInResult.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", externalLoginInfo.Principal.Identity.Name, externalLoginInfo.LoginProvider);

            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        ReturnUrl = returnUrl;
        ProviderDisplayName = externalLoginInfo.ProviderDisplayName;

        if (externalLoginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            Form = new ExternalLoginInput { Email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = default)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLoginInfo? externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo is null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // TODO: Replace with Fluent Validation validators.
        if (ModelState.IsValid)
        {
            User user = CreateUser();

            await _userStore.SetUserNameAsync(user, Form.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Form.Email, CancellationToken.None);

            IdentityResult result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, externalLoginInfo);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", externalLoginInfo.LoginProvider);

                    string userId = await _userManager.GetUserIdAsync(user);
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    string? callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId, code },
                        Request.Scheme);

                    await _emailSender.SendEmailAsync(Form.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Form.Email });
                    }

                    await _signInManager.SignInAsync(user, false, externalLoginInfo.LoginProvider);

                    return LocalRedirect(returnUrl);
                }
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(Empty, error.Description);
            }
        }

        ProviderDisplayName = externalLoginInfo.ProviderDisplayName;
        ReturnUrl = returnUrl;

        return Page();
    }

    private User CreateUser()
    {
        try
        {
            return Activator.CreateInstance<User>();
        }
        catch
        {
            throw new InvalidOperationException(
                $"Can't create an instance of '{nameof(User)}'. " +
                $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
        }
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<User>)_userStore;
    }
}
