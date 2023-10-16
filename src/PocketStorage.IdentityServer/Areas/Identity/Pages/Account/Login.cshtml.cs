using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using static System.String;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly SignInManager<User> _signInManager;

    public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty] public LoginInput Form { get; set; } = default!;

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public string? ReturnUrl { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? returnUrl = default)
    {
        if (!IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = default)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        SignInResult result = await _signInManager.PasswordSignInAsync(Form.Email, Form.Password, Form.RememberMe, false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");

            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Form.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");

            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(Empty, "Invalid login attempt.");

        return Page();
    }
}
