using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Application.Models;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class EnableAuthenticatorModel : PageModel
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    private readonly UrlEncoder _urlEncoder;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<EnableAuthenticatorInput> _validator;

    public EnableAuthenticatorModel(
        UserManager<User> userManager,
        UrlEncoder urlEncoder,
        IValidator<EnableAuthenticatorInput> validator)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
        _validator = validator;
    }

    public string? SharedKey { get; set; }
    public string? AuthenticatorUri { get; set; }

    [TempData] public string[]? RecoveryCodes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public EnableAuthenticatorInput Form { get; set; } = null!;

    public Dictionary<string, string[]> Errors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadSharedKeyAndQrCodeUriAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Errors = (await _validator.ValidateAsync(new ValidationContext<EnableAuthenticatorInput>(Form))).DistinctErrorsByProperty();
        if (Errors.Count > 0)
        {
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        string token = Form.Code.Replace(" ", Empty).Replace("-", Empty);

        bool hasValidToken = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, token);
        if (!hasValidToken)
        {
            Errors = new Dictionary<string, string[]> { [nameof(Form.Code)] = new[] { "Verification code is invalid." } };

            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        StatusMessage = "Your authenticator app has been verified.";

        if (await _userManager.CountRecoveryCodesAsync(user) != 0)
        {
            return RedirectToPage("./TwoFactorAuthentication");
        }

        RecoveryCodes = (await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray();

        return RedirectToPage("./ShowRecoveryCodes");
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(User user)
    {
        string? unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey);

        string? email = await _userManager.GetEmailAsync(user);

        AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
    }

    private static string FormatKey(string unformattedKey)
    {
        StringBuilder result = new();

        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey) =>
        Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            _urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
            _urlEncoder.Encode(email),
            unformattedKey);
}
