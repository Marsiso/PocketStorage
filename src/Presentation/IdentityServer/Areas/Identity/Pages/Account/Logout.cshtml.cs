﻿using Domain.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Areas.Identity.Pages.Account;

public sealed class LogoutModel : PageModel
{
    #region Private Fields

    private readonly ILogger<LogoutModel> _logger;
    private readonly SignInManager<ApplicationUserEntity> _signInManager;

    #endregion Private Fields

    #region Public Constructors

    public LogoutModel(SignInManager<ApplicationUserEntity> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new request and the
            // identity for the user gets updated.
            return RedirectToPage();
        }
    }

    #endregion Public Methods
}