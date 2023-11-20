using System.Text;
using System.Text.Encodings.Web;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<ResendEmailConfirmationInput> _validator;

    public ResendEmailConfirmationModel(UserManager<User> userManager, IEmailSender emailSender, IValidator<ResendEmailConfirmationInput> validator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
    }

    [BindProperty] public ResendEmailConfirmationInput Form { get; set; } = null!;

    public Dictionary<string, string[]> Errors { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<ResendEmailConfirmationInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        User? user = await _userManager.FindByEmailAsync(Form.Email);
        if (user == null)
        {
            Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = new[] { "Verification email sent. Please check your email." } };
            return Page();
        }

        string identifier = await _userManager.GetUserIdAsync(user);
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        string? callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { userId = identifier, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)) },
            Request.Scheme);

        await _emailSender.SendEmailAsync(
            Form.Email,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = new[] { "Verification email sent. Please check your email." } };
        return Page();
    }
}
