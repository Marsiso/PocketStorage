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

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<NewEmailInput> _validator;

    public EmailModel(
        UserManager<User> userManager,
        IEmailSender emailSender,
        IValidator<NewEmailInput> validator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
    }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public NewEmailInput Form { get; set; } = null!;

    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    private async Task LoadAsync(User user)
    {
        string? email = await _userManager.GetEmailAsync(user);

        Email = email;
        Form = new NewEmailInput { NewEmail = email };
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
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

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Errors = (await _validator.ValidateAsync(new ValidationContext<NewEmailInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            await LoadAsync(user);
            return Page();
        }

        string? email = await _userManager.GetEmailAsync(user);
        if (Form.NewEmail != email)
        {
            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateChangeEmailTokenAsync(user, Form.NewEmail);

            string? callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                null,
                new { area = "Identity", userId, email = Form.NewEmail, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)) },
                Request.Scheme);

            await _emailSender.SendEmailAsync(
                Form.NewEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Confirmation link to change email sent. Please check your email.";

            return RedirectToPage();
        }

        StatusMessage = "Your email is unchanged.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Errors = (await _validator.ValidateAsync(new ValidationContext<NewEmailInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            await LoadAsync(user);
            return Page();
        }

        string userId = await _userManager.GetUserIdAsync(user);
        string? email = await _userManager.GetEmailAsync(user);
        string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        string? callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { area = "Identity", userId, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)) },
            Request.Scheme);

        await _emailSender.SendEmailAsync(
            email,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        StatusMessage = "Verification email sent. Please check your email.";

        return RedirectToPage();
    }
}
