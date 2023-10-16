using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class DeletePersonalDataModel : PageModel
{
    private readonly ILogger<DeletePersonalDataModel> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public DeletePersonalDataModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<DeletePersonalDataModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }


    [BindProperty] public DeletePersonalDataInput Form { get; set; }

    public bool RequirePassword { get; set; }

    public async Task<IActionResult> OnGet()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        if (RequirePassword)
        {
            if (!await _userManager.CheckPasswordAsync(user, Form.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return Page();
            }
        }

        IdentityResult result = await _userManager.DeleteAsync(user);
        string userId = await _userManager.GetUserIdAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        return Redirect("~/");
    }
}
