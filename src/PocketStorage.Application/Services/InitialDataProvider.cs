using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Settings = PocketStorage.Domain.Options.Settings;

namespace PocketStorage.Application.Services;

public class InitialDataProvider(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

        Settings settings = scope.ServiceProvider.GetRequiredService<Settings>();
        IWebHostEnvironment environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();

        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        IOpenIddictApplicationManager applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (environment.IsDevelopment())
        {
            await context.Database.EnsureDeletedAsync(cancellationToken);
        }

        await context.Database.MigrateAsync(cancellationToken);

        if (environment.IsDevelopment())
        {
            await SeedIdentityData(settings, userManager);
            await SeedOpenIdDictData(settings, applicationManager, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedIdentityData(Settings settings, UserManager<User> manager)
    {
        foreach (AspNetIdentityUserSettings identityUser in settings.Identity.Users)
        {
            await manager.CreateAsync(new User
            {
                GivenName = identityUser.GivenName,
                FamilyName = identityUser.FamilyName,
                UserName = identityUser.Email,
                Email = identityUser.Email,
                EmailConfirmed = identityUser.EmailVerified,
                PhoneNumber = identityUser.PhoneNumber,
                PhoneNumberConfirmed = identityUser.PhoneNumberVerified,
                Culture = identityUser.Locale
            });

            User user = await manager.FindByEmailAsync(identityUser.Email) ?? throw new EntityNotFoundException(identityUser.Email, nameof(User));

            await manager.AddPasswordAsync(user, identityUser.Password);

            foreach (string role in identityUser.Roles)
            {
                await manager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedOpenIdDictData(Settings settings, IOpenIddictApplicationManager manager, CancellationToken cancellationToken)
    {
        foreach (OpenIdConnectClientSettings application in settings.OpenIdConnect.Clients)
        {
            OpenIddictApplicationDescriptor descriptor = new() { ClientId = application.ClientId, ClientSecret = application.ClientSecret, DisplayName = application.DisplayName, ConsentType = ConsentTypes.Explicit };

            application.Scopes.Select(scope => Permissions.Prefixes.Scope + scope).ToList().ForEach(scope => descriptor.Permissions.Add(scope));
            application.Endpoints.Select(endpoint => Permissions.Prefixes.Endpoint + endpoint).ToList().ForEach(endpoint => descriptor.Permissions.Add(endpoint));
            application.GrantTypes.Select(grantType => Permissions.Prefixes.GrantType + grantType).ToList().ForEach(grantType => descriptor.Permissions.Add(grantType));
            application.ResponseTypes.Select(responseType => Permissions.Prefixes.ResponseType + responseType).ToList().ForEach(responseType => descriptor.Permissions.Add(responseType));

            application.Requirements.Select(requirement => Requirements.Prefixes.Feature + requirement).ToList().ForEach(requirement => descriptor.Requirements.Add(requirement));

            application.RedirectUris.Select(redirectUri => new Uri(redirectUri)).ToList().ForEach(redirectUri => descriptor.RedirectUris.Add(redirectUri));
            application.PostLogoutRedirectUris.Select(redirectUri => new Uri(redirectUri)).ToList().ForEach(redirectUri => descriptor.PostLogoutRedirectUris.Add(redirectUri));

            await manager.CreateAsync(descriptor, cancellationToken);
        }
    }
}
