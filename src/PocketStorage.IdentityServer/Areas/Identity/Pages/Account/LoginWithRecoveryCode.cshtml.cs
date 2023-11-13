using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly IValidator<LoginWithRecoveryCodeInput> _validator;

    public LoginWithRecoveryCodeModel(SignInManager<User> signInManager, IValidator<LoginWithRecoveryCodeInput> validator)
    {
        _signInManager = signInManager;
        _validator = validator;
    }

    [BindProperty] public LoginWithRecoveryCodeInput Form { get; set; } = null!;

    public string? ReturnUrl { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<LoginWithRecoveryCodeInput>(Form))).DistinctErrorsByProperty();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        User? user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        string recoveryCode = Form.RecoveryCode.Replace(" ", string.Empty);

        SignInResult result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        Errors = new Dictionary<string, string[]> { [nameof(Form.RecoveryCode)] = new[] { "Invalid recovery code entered." } };

        return Page();
    }
}
