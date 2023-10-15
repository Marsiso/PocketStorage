using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<User> _userManager;

    public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty] public ForgotPasswordInput Form { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync()
    {
        // TODO: Replace with Fluent Validation validators.
        if (!ModelState.IsValid)
        {
            return Page();
        }

        User? user = await _userManager.FindByEmailAsync(Form.Email);
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        string callbackUrl = Url.Page(
            "/Account/ResetPassword",
            null,
            new { area = "Identity", code },
            Request.Scheme);

        await _emailSender.SendEmailAsync(
            Form.Email,
            "Reset Password",
            $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
