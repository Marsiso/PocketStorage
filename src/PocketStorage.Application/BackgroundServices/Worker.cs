using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Exceptions;
using PocketStorage.Domain.Options;

namespace PocketStorage.Application.BackgroundServices;

public class Worker(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        await using DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();
        IOpenIddictApplicationManager applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        ApplicationSettings settings = scope.ServiceProvider.GetRequiredService<ApplicationSettings>();
        UserManager<User> manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await context.Database.MigrateAsync(cancellationToken);
        await GenerateAspNetIdentityUsers(settings, manager, cancellationToken);
        await GenerateOpenIdConnectClients(settings, applicationManager, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task GenerateAspNetIdentityUsers(ApplicationSettings settings, UserManager<User> manager, CancellationToken cancellationToken)
    {
        foreach (AspNetIdentityUserSettings user in settings.AspNet.Identity.Users)
        {
            User? originalUser = await manager.FindByEmailAsync(user.Email);
            if (originalUser != null)
            {
                await manager.DeleteAsync(originalUser);
            }

            await manager.CreateAsync(new User
            {
                GivenName = user.GivenName,
                FamilyName = user.FamilyName,
                UserName = user.Email,
                Email = user.Email,
                EmailConfirmed = user.EmailVerified,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberVerified,
                Culture = user.Locale
            });

            User createdUser = await manager.FindByEmailAsync(user.Email) ?? throw new EntityNotFoundException(user.Email, nameof(User));

            await manager.AddPasswordAsync(createdUser, user.Password);

            foreach (string role in user.Roles)
            {
                await manager.AddToRoleAsync(createdUser, role);
            }
        }
    }

    private static async Task GenerateOpenIdConnectClients(ApplicationSettings settings, IOpenIddictApplicationManager applicationManager, CancellationToken cancellationToken)
    {
        foreach (OpenIdConnectClientSettings client in settings.OpenIdConnect.Clients)
        {
            object? application = await applicationManager.FindByClientIdAsync(client.Id, cancellationToken);

            if (application != null)
            {
                await applicationManager.DeleteAsync(application, cancellationToken);
            }

            OpenIddictApplicationDescriptor descriptor = new() { ClientId = client.Id, ClientSecret = client.Secret, ConsentType = OpenIddictConstants.ConsentTypes.Explicit, DisplayName = client.DisplayName };

            foreach (string scope in client.Scopes.Distinct().Where(scope => descriptor.Permissions.All(s => OpenIddictConstants.Permissions.Prefixes.Scope + scope != s)))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            foreach (string endpoint in client.Endpoints.Distinct().Where(endpoint => descriptor.Permissions.All(e => OpenIddictConstants.Permissions.Prefixes.Endpoint + endpoint != e)))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Endpoint + endpoint);
            }

            foreach (string grantType in client.GrantTypes.Distinct().Where(grantType => descriptor.Permissions.All(g => OpenIddictConstants.Permissions.Prefixes.GrantType + grantType != g)))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
            }

            foreach (string responseType in client.ResponseTypes.Distinct().Where(responseType => descriptor.Permissions.All(r => OpenIddictConstants.Permissions.Prefixes.ResponseType + responseType != r)))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.ResponseType + responseType);
            }

            foreach (string requirement in client.Requirements.Distinct().Where(requirement => descriptor.Requirements.All(r => OpenIddictConstants.Requirements.Prefixes.Feature + requirement != r)))
            {
                descriptor.Requirements.Add(OpenIddictConstants.Requirements.Prefixes.Feature + requirement);
            }

            foreach (string uri in client.RedirectUris.Distinct().Where(endpoint => descriptor.RedirectUris.All(e => new Uri(endpoint) != e)))
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }

            foreach (string uri in client.PostLogoutRedirectUris.Distinct().Where(endpoint => descriptor.PostLogoutRedirectUris.All(e => new Uri(endpoint) != e)))
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            }

            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
    }
}
