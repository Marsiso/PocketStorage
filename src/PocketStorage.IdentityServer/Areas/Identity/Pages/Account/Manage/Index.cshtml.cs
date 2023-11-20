using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<PhoneInput> _validator;

    public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager, IValidator<PhoneInput> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }


    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public PhoneInput Form { get; set; } = default!;

    public string? Username { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

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
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Errors = (await _validator.ValidateAsync(new ValidationContext<PhoneInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            await LoadAsync(user);
            return Page();
        }

        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Form.PhoneNumber != phoneNumber)
        {
            IdentityResult result = await _userManager.SetPhoneNumberAsync(user, Form.PhoneNumber);
            if (!result.Succeeded)
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
