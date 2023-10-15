using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.DataTransferObjects;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetPasswordModel(UserManager<IdentityUser> userManager) => _userManager = userManager;

    [BindProperty] public ResetPasswordInput Form { get; set; } = default!;

    public IActionResult OnGet(string? code = default)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        IdentityUser? user = await _userManager.FindByEmailAsync(Form.Email);
        if (user is null)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, Form.Code, Form.Password);
        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(Empty, error.Description);
        }

        return Page();
    }
}
