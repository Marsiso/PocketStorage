using Microsoft.AspNetCore.Authentication.Cookies;

namespace PocketStorage.Application.Extensions;

public static class CookieAuthenticationOptionsExtensions
{
    public static void Configure(this CookieAuthenticationOptions options)
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);

        options.LoginPath = "/identity/account/login";
        options.AccessDeniedPath = "/identity/account/accessDenied";

        options.SlidingExpiration = true;
    }
}
