using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string? Username { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public PhoneInput Form { get; set; } = default!;

    private async Task LoadAsync(User user)
    {
        string? userName = await _userManager.GetUserNameAsync(user);
        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Username = userName;

        Form = new PhoneInput { PhoneNumber = phoneNumber };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Form.PhoneNumber != phoneNumber)
        {
            IdentityResult setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Form.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your profile has been updated";

        return RedirectToPage();
    }
}
