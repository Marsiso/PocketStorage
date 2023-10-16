using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class SetPasswordModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public SetPasswordModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }


    [BindProperty] public SetPasswordInput Form { get; set; } = default!;

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await _userManager.HasPasswordAsync(user);

        if (hasPassword)
        {
            return RedirectToPage("./ChangePassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult identityResult = await _userManager.AddPasswordAsync(user, Form.NewPassword);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your password has been set.";

        return RedirectToPage();
    }
}
