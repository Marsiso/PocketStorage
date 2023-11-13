using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IValidator _validator;

    public LoginWith2faModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IValidator<LoginWithTwoFactorAuthInput> validator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _validator = validator;
    }

    [BindProperty] public LoginWithTwoFactorAuthInput Form { get; set; } = null!;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<LoginWithTwoFactorAuthInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        string authenticatorCode = Form.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        SignInResult result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Form.RememberMachine);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        Errors = new Dictionary<string, string[]> { [nameof(Form.TwoFactorCode)] = new[] { "Invalid authenticator code." } };

        return Page();
    }
}
