using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using PocketStorage.Domain.Constants;

namespace PocketStorage.Application.Extensions;

public static class AntiforgeryOptionsExtensions
{
    public static AntiforgeryOptions Configure(this AntiforgeryOptions options)
    {
        options.HeaderName = AntiforgeryDefaults.HeaderName;
        options.Cookie.Name = AntiforgeryDefaults.CookieName;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        return options;
    }
}
