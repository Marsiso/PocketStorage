using PocketStorage.Domain.Contracts;

namespace PocketStorage.ResourceServer.Extensions;

public static class AntiforgeryServiceExtensions
{
    public static IServiceCollection AddApplicationAntiforgery(this IServiceCollection services) =>
        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryDefaults.HeaderName;
            options.Cookie.Name = AntiforgeryDefaults.CookieName;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
}
