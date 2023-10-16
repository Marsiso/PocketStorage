using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.Models;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public ConfirmEmailModel(UserManager<User> userManager) => _userManager = userManager;

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (IsNullOrWhiteSpace(userId) || IsNullOrWhiteSpace(code))
        {
            return RedirectToPage("/Index");
        }

        User? user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

        StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";

        return Page();
    }
}
