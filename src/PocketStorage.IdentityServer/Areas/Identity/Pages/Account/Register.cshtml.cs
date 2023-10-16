using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;

    public RegisterModel(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }


    [BindProperty] public RegisterInput Form { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();


    public async Task OnGetAsync(string? returnUrl = default)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = default)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        User user = CreateUser();

        await _userStore.SetUserNameAsync(user, Form.Email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Form.Email, CancellationToken.None);
        IdentityResult result = await _userManager.CreateAsync(user, Form.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", userId, code, returnUrl },
                Request.Scheme);

            await _emailSender.SendEmailAsync(Form.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Form.Email, returnUrl });
            }

            await _signInManager.SignInAsync(user, false);

            return LocalRedirect(returnUrl);
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

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
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
