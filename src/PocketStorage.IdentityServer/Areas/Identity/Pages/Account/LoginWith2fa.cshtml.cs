using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel
{
    private readonly ILogger<LoginWith2faModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public LoginWith2faModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty] public LoginWithTwoFactorAuthInput Form { get; set; } = default!;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = default)
    {
        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user is null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        string authenticatorCode = Form.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        SignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Form.RememberMachine);

        string userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", userId);

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", userId);

            return RedirectToPage("./Lockout");
        }

        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", userId);

        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");

        return Page();
    }
}
