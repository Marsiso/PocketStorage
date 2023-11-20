using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using static System.String;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly IValidator<LoginInput> _validator;

    public LoginModel(SignInManager<User> signInManager, IValidator<LoginInput> validator)
    {
        _signInManager = signInManager;
        _validator = validator;
    }

    [BindProperty] public LoginInput Form { get; set; } = null!;

    [TempData] public string? ErrorMessage { get; set; }

    public string? ReturnUrl { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!IsNullOrEmpty(ErrorMessage))
        {
            Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = new[] { ErrorMessage } };
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        Errors = (await _validator.ValidateAsync(new ValidationContext<LoginInput>(Form))).DistinctErrorsByProperty();

        if (Errors.Count > 0)
        {
            return Page();
        }

        SignInResult result = await _signInManager.PasswordSignInAsync(Form.Email, Form.Password, Form.RememberMe, false);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Form.RememberMe });
        }

        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = new[] { "Invalid login attempt." } };

        return Page();
    }
}
