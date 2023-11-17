using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Constants;
using PocketStorage.Domain.Exceptions;

namespace PocketStorage.Application.BackgroundServices;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        await using DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        IOpenIddictApplicationManager applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await context.Database.MigrateAsync(cancellationToken);
        await GenerateUserData(userManager, cancellationToken);
        await GenerateBlazorWebAssemblyClientData(configuration, applicationManager, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task GenerateUserData(UserManager<User> manager, CancellationToken cancellationToken)
    {
        if (!await manager.Users.AnyAsync(user => user.Email == "system.administrator@prov.dev", cancellationToken))
        {
            await manager.CreateAsync(new User
            {
                GivenName = "System",
                FamilyName = RoleDefaults.Administrator,
                UserName = "system.administrator@prov.dev",
                Email = "system.administrator@prov.dev",
                Culture = CultureDefaults.Default
            });

            User user = await manager.FindByEmailAsync("system.administrator@prov.dev") ?? throw new EntityNotFoundException("system.administrator@prov.dev", nameof(User));

            await manager.AddPasswordAsync(user, "Password123$");

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Administrator))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Administrator);
            }

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Manager))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Manager);
            }

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Default))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Default);
            }
        }

        if (!await manager.Users.AnyAsync(user => user.Email == "system.manager@prov.dev", cancellationToken))
        {
            await manager.CreateAsync(new User
            {
                GivenName = "System",
                FamilyName = RoleDefaults.Manager,
                UserName = "system.manager@prov.dev",
                Email = "system.manager@prov.dev",
                Culture = CultureDefaults.Default
            });

            User user = await manager.FindByEmailAsync("system.manager@prov.dev") ?? throw new EntityNotFoundException("system.administrator@prov.dev", nameof(User));

            await manager.AddPasswordAsync(user, "Password123$");

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Manager))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Manager);
            }

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Default))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Default);
            }
        }

        if (!await manager.Users.AnyAsync(user => user.Email == "system.default@prov.dev", cancellationToken))
        {
            await manager.CreateAsync(new User
            {
                GivenName = "System",
                FamilyName = RoleDefaults.Default,
                UserName = "system.default@prov.dev",
                Email = "system.default@prov.dev",
                Culture = CultureDefaults.Default
            });

            User user = await manager.FindByEmailAsync("system.default@prov.dev") ?? throw new EntityNotFoundException("system.administrator@prov.dev", nameof(User));

            await manager.AddPasswordAsync(user, "Password123$");

            if (!await manager.IsInRoleAsync(user, RoleDefaults.Default))
            {
                await manager.AddToRoleAsync(user, RoleDefaults.Default);
            }
        }
    }

    private static async Task GenerateBlazorWebAssemblyClientData(IConfiguration configuration, IOpenIddictApplicationManager applicationManager, CancellationToken cancellationToken)
    {
        string? clientId = configuration["OpenIdConnect:Clients:BlazorWebAssembly:ClientId"];
        string? clientSecret = configuration["OpenIdConnect:Clients:BlazorWebAssembly:ClientSecret"];
        string? clientDisplayName = configuration["OpenIdConnect:Clients:BlazorWebAssembly:DisplayName"];
        string? redirectUri = configuration["OpenIdConnect:Clients:BlazorWebAssembly:RedirectUri"];
        string? postLogoutRedirectUri = configuration["OpenIdConnect:Clients:BlazorWebAssembly:PostLogoutRedirectUri"];

        object? client = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        if (client == null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = clientDisplayName,
                RedirectUris = { new Uri(redirectUri) },
                PostLogoutRedirectUris = { new Uri(postLogoutRedirectUri) },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.Name,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.Email,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.PhoneNumber,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.Locale,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.Zoneinfo,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictScopeDefaults.UpdatedAt,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
                },
                Requirements = { OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange }
            }, cancellationToken);
        }
    }
}
