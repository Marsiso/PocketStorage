namespace PocketStorage.ResourceServer.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapNotFound(this IEndpointRouteBuilder endpointRouteBuilder, string pattern)
    {
        endpointRouteBuilder.Map(pattern, context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        return endpointRouteBuilder;
    }

    public static IEndpointRouteBuilder MapUnauthorized(this IEndpointRouteBuilder endpointRouteBuilder, string pattern)
    {
        endpointRouteBuilder.Map(pattern, context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        });

        return endpointRouteBuilder;
    }
}
