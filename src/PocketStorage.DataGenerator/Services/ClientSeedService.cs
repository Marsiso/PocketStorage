using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PocketStorage.DataGenerator.Services;

public sealed class ClientSeedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ClientSeedService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

        IOpenIddictApplicationManager applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string clientId = configuration["Clients:BlazorWebAssembly:Id"] ?? string.Empty;
        string clientSecret = configuration["Clients:BlazorWebAssembly:Secret"] ?? string.Empty;
        string clientDisplayName = configuration["Clients:BlazorWebAssembly:DisplayName"] ?? string.Empty;

        object? client = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = clientDisplayName,
                RedirectUris = { new Uri("https://localhost:7234/signin-oidc") },
                PostLogoutRedirectUris = { new Uri("https://localhost:7234/signout-callback-oidc") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + Scopes.OfflineAccess
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
