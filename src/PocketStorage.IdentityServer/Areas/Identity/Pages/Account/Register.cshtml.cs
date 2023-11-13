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
using PocketStorage.Domain.Constants;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IEmailSender _emailSender;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IValidator<RegisterInput> _validator;

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserStore<User> userStore,
        IEmailSender emailSender,
        IValidator<RegisterInput> validator)
    {
        _userStore = userStore;
        _userManager = userManager;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _emailSender = emailSender;
        _validator = validator;
    }


    [BindProperty] public RegisterInput Form { get; set; } = null!;

    public string? ReturnUrl { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        Errors = (await _validator.ValidateAsync(new ValidationContext<RegisterInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        User user = new() { GivenName = Form.GivenName, FamilyName = Form.FamilyName, Culture = CultureDefaults.Default };

        await _userStore.SetUserNameAsync(user, Form.Email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Form.Email, CancellationToken.None);

        IdentityResult result = await _userManager.CreateAsync(user, Form.Password);
        if (result.Succeeded)
        {
            string identifier = await _userManager.GetUserIdAsync(user);
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string? callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new { area = "Identity", userId = identifier, code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)), returnUrl },
                Request.Scheme);

            await _emailSender.SendEmailAsync(Form.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Form.Email, returnUrl });
            }

            await _signInManager.SignInAsync(user, false);
            return LocalRedirect(returnUrl);
        }

        Errors = new Dictionary<string, string[]> { [nameof(Form.Email)] = result.Errors.Select(error => error.Description).ToArray() };

        return Page();
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<User>)_userStore;
    }
}
