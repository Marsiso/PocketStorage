using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IValidator<ResetPasswordInput> _validator;

    public ResetPasswordModel(UserManager<IdentityUser> userManager, IValidator<ResetPasswordInput> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    [BindProperty] public ResetPasswordInput Form { get; set; } = null!;

    public Dictionary<string, string[]> Errors { get; set; } = new();

    public IActionResult OnGet(string? code = null)
    {
        if (IsNullOrWhiteSpace(code))
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        Form = new ResetPasswordInput { Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)) };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<ResetPasswordInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        IdentityUser? user = await _userManager.FindByEmailAsync(Form.Email);
        if (user == null)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, Form.Code, Form.Password);
        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = result.Errors.Select(error => error.Description).ToArray() };

        return Page();
    }
}
