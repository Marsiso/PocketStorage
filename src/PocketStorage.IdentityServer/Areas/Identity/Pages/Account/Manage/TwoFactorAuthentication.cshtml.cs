using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel
{
    private readonly ILogger<TwoFactorAuthenticationModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public TwoFactorAuthenticationModel(
        UserManager<User> userManager, SignInManager<User> signInManager, ILogger<TwoFactorAuthenticationModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }

    [BindProperty] public bool Is2FaEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _signInManager.ForgetTwoFactorClientAsync();

        StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";

        return RedirectToPage();
    }
}
