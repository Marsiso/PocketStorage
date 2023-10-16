using Microsoft.Net.Http.Headers;

namespace PocketStorage.ResourceServer.Extensions;

public static class BuilderExtensions
{
    public static IApplicationBuilder UseNoUnauthorizedRedirect(this IApplicationBuilder applicationBuilder, params string[] segments)
    {
        applicationBuilder.Use(async (httpContext, func) =>
        {
            if (segments.Any(segment => httpContext.Request.Path.StartsWithSegments(segment)))
            {
                httpContext.Request.Headers[HeaderNames.XRequestedWith] = "XMLHttpRequest";
            }

            await func();
        });

        return applicationBuilder;
    }
}
