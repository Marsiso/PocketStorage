using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.Models;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public RegisterConfirmationModel(UserManager<User> userManager) => _userManager = userManager;

    public string? Email { get; set; }
    public string? EmailConfirmationUrl { get; set; }
    public bool DisplayConfirmAccountLink { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
    {
        if (IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Index");
        }

        returnUrl ??= Url.Content("~/");

        User? user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        Email = email;
        DisplayConfirmAccountLink = true;

        if (!DisplayConfirmAccountLink)
        {
            return Page();
        }

        string identifier = await _userManager.GetUserIdAsync(user);
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        EmailConfirmationUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { area = "Identity", userId = identifier, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)), returnUrl },
            Request.Scheme);

        return Page();
    }
}
