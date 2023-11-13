using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class GenerateRecoveryCodesModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public GenerateRecoveryCodesModel(UserManager<User> userManager) => _userManager = userManager;

    [TempData] public string[]? RecoveryCodes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!hasTwoFactorEnabled)
        {
            throw new InvalidOperationException("Cannot generate recovery codes for user because they do not have 2FA enabled.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!hasTwoFactorEnabled)
        {
            throw new InvalidOperationException("Cannot generate recovery codes for user as they do not have 2FA enabled.");
        }

        RecoveryCodes = (await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray();
        StatusMessage = "You have generated new recovery codes.";

        return RedirectToPage("./ShowRecoveryCodes");
    }
}
