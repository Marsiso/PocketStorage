using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.ResourceServer.Controllers.Base;
using static System.String;

namespace PocketStorage.ResourceServer.Controllers;

public class CultureController(ILogger<CultureController> logger) : ApiControllerBase<CultureController>(logger)
{
    [HttpPost("~/api/culture")]
    public IActionResult SetCulture(string? culture, string? redirectUri)
    {
        if (IsNullOrWhiteSpace(culture))
        {
            return BadRequest();
        }

        RequestCulture cultureInfo = new RequestCulture(culture);
        HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(cultureInfo));

        return Ok();
    }
}
