using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using PocketStorage.BFF.Authorization.Constants;

namespace PocketStorage.Application.Extensions;

public static class AntiforgeryOptionsExtensions
{
    public static AntiforgeryOptions Configure(this AntiforgeryOptions options, bool development)
    {
        options.HeaderName = AntiforgeryConstants.HeaderName;
        options.Cookie.Name = AntiforgeryConstants.CookieName;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        return options;
    }
}
