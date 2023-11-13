using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class PersonalDataModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public PersonalDataModel(UserManager<User> userManager) => _userManager = userManager;

    public async Task<IActionResult> OnGet()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return Page();
    }
}
