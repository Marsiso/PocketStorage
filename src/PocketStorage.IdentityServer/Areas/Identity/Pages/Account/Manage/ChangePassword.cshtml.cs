using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<ChangePasswordInput> _validator;

    public ChangePasswordModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IValidator<ChangePasswordInput> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }

    [BindProperty] public ChangePasswordInput Form { get; set; } = null!;

    [TempData] public string? StatusMessage { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToPage("./SetPassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<ChangePasswordInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, Form.OldPassword, Form.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (IdentityError error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your password has been changed.";

        return RedirectToPage();
    }
}
