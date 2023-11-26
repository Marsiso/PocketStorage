using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.ResourceServer.Controllers.Base;

namespace PocketStorage.ResourceServer.Controllers;

public class AccountController : ApiControllerBase<AccountController>
{
    public AccountController(ILogger<AccountController> logger) : base(logger)
    {
    }

    [HttpGet("~/api/account/sign-in")]
    public ActionResult Login(string returnUrl) => Challenge(new AuthenticationProperties { RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/" });

    [ValidateAntiForgeryToken]
    [Authorize]
    [HttpPost("~/api/account/sign-out")]
    public IActionResult Logout() => SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
}
