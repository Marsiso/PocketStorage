using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class SetPasswordModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<SetPasswordInput> _validator;

    public SetPasswordModel(UserManager<User> userManager, SignInManager<User> signInManager, IValidator<SetPasswordInput> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }

    [BindProperty] public SetPasswordInput Form { get; set; } = default!;

    [TempData] public string? StatusMessage { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
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
        Errors = (await _validator.ValidateAsync(new ValidationContext<SetPasswordInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult result = await _userManager.AddPasswordAsync(user, Form.NewPassword);
        if (!result.Succeeded)
        {
            Errors = new Dictionary<string, string[]> { [nameof(Form.NewPassword)] = result.Errors.Select(error => error.Description).ToArray() };
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Your password has been set.";

        return RedirectToPage();
    }
}
