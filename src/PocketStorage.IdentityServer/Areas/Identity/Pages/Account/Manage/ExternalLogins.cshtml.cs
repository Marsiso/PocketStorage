﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;
using static System.String;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class ExternalLoginsModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;

    public ExternalLoginsModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserStore<User> userStore)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userStore = userStore;
    }


    public IList<UserLoginInfo> CurrentLogins { get; set; } = new List<UserLoginInfo>();
    public IList<AuthenticationScheme> OtherLogins { get; set; } = new List<AuthenticationScheme>();
    public bool ShowRemoveButton { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        CurrentLogins = await _userManager.GetLoginsAsync(user);
        OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();

        string? passwordHash = default;
        if (_userStore is IUserPasswordStore<User> userPasswordStore)
        {
            passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
        }

        ShowRemoveButton = !IsNullOrWhiteSpace(passwordHash) || CurrentLogins.Count > 1;

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
        {
            StatusMessage = "The external login was not removed.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "The external login was removed.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        string? redirectUrl = Url.Page("./ExternalLogins", "LinkLoginCallback");

        AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));

        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        string userId = await _userManager.GetUserIdAsync(user);

        ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync(userId);
        if (loginInfo is null)
        {
            throw new InvalidOperationException("Unexpected error occurred loading external login info.");
        }

        IdentityResult result = await _userManager.AddLoginAsync(user, loginInfo);
        if (!result.Succeeded)
        {
            StatusMessage = "The external login was not added. External logins can only be associated with one account.";
            return RedirectToPage();
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        StatusMessage = "The external login was added.";

        return RedirectToPage();
    }
}
