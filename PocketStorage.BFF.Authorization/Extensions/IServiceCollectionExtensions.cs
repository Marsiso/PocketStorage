using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PocketStorage.BFF.Authorization.Services;

namespace PocketStorage.BFF.Authorization.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddSingleton<IAuthorizationHandler, PermitHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermitPolicyProvider>();

        return services;
    }
}
