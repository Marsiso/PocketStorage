using System.Text;
using System.Text.Encodings.Web;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<ForgotPasswordInput> _validator;

    public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender, IValidator<ForgotPasswordInput> validator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
    }

    [BindProperty] public ForgotPasswordInput Form { get; set; } = null!;

    public Dictionary<string, string[]> Errors { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<ForgotPasswordInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        User? user = await _userManager.FindByEmailAsync(Form.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        string? callbackUrl = Url.Page(
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
