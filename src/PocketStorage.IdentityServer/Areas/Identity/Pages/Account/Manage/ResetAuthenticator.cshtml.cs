using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class ResetAuthenticatorModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public ResetAuthenticatorModel(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
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

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

        return RedirectToPage("./EnableAuthenticator");
    }
}
