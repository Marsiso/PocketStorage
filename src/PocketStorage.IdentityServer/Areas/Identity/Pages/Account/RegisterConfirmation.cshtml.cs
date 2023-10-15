using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly IEmailSender _sender;
    private readonly UserManager<User> _userManager;

    public RegisterConfirmationModel(UserManager<User> userManager, IEmailSender sender)
    {
        _userManager = userManager;
        _sender = sender;
    }

    public string? Email { get; set; }
    public bool DisplayConfirmAccountLink { get; set; }
    public string? EmailConfirmationUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = default)
    {
        if (email is null)
        {
            return RedirectToPage("/Index");
        }

        returnUrl ??= Url.Content("~/");

        User? user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        Email = email;
        DisplayConfirmAccountLink = true;

        if (!DisplayConfirmAccountLink)
        {
            return Page();
        }

        string userId = await _userManager.GetUserIdAsync(user);
        string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        EmailConfirmationUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { area = "Identity", userId, code, returnUrl },
            Request.Scheme);

        return Page();
    }
}
