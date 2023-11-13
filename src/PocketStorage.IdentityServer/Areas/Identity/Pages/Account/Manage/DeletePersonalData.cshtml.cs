using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class DeletePersonalDataModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<DeletePersonalDataInput> _validator;

    public DeletePersonalDataModel(UserManager<User> userManager, SignInManager<User> signInManager, IValidator<DeletePersonalDataInput> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }


    [BindProperty] public DeletePersonalDataInput Form { get; set; } = null!;

    public bool RequirePassword { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

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
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        if (RequirePassword)
        {
            Errors = (await _validator.ValidateAsync(new ValidationContext<DeletePersonalDataInput>(Form))).DistinctErrorsByProperty();
            if (Errors.Count > 0)
            {
                return Page();
            }
        }

        IdentityResult result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }

        await _signInManager.SignOutAsync();
        return Redirect("~/");
    }
}
