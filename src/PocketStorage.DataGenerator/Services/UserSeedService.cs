using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PocketStorage.Data;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Enums;

namespace PocketStorage.DataGenerator.Services;

public sealed class UserSeedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public UserSeedService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope serviceScope = _serviceProvider.CreateAsyncScope();

        DataContext databaseContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
        UserManager<User> userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        RoleManager<Role> roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        IPasswordHasher<User> passwordHasher = serviceScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await databaseContext.Database.EnsureDeletedAsync(cancellationToken);
        await databaseContext.Database.EnsureCreatedAsync(cancellationToken);

        await SeedAsync(userManager, roleManager, passwordHasher, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager, IPasswordHasher<User> passwordHasher, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(roleManager);
        await SeedUserAsync(userManager, roleManager, passwordHasher, cancellationToken);
    }

    private async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        List<Role> roles = new() { new Role { Name = "Administrator", Permissions = Permission.All }, new Role { Name = "Manager", Permissions = Permission.All }, new Role { Name = "Default", Permissions = Permission.All }, new Role { Name = "Visitor", Permissions = Permission.None } };

        foreach (Role role in roles)
        {
            Role? originalRole = await roleManager.FindByNameAsync(role.Name);

            if (originalRole is null)
            {
                await roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedUserAsync(UserManager<User> userManager, RoleManager<Role> roleManager, IPasswordHasher<User> passwordHasher, CancellationToken cancellationToken)
    {
        List<Role> roles = await roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);

        List<User> users = new()
        {
            new User
            {
                Email = "system.administrator@example.dev",
                UserName = "system.administrator@example.dev",
                GivenName = "System",
                FamilyName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            },
            new User
            {
                Email = "system.manager@example.dev",
                UserName = "system.manager@example.dev",
                GivenName = "System",
                FamilyName = "Manager",
                EmailConfirmed = true,
                IsActive = true
            },
            new User
            {
                Email = "default.access@example.dev",
                UserName = "default.access@example.dev",
                GivenName = "Default",
                FamilyName = "Access",
                EmailConfirmed = true,
                IsActive = true
            },
            new User
            {
                Email = "visitor.noaccess@example.dev",
                UserName = "visitor.noaccess@example.dev",
                GivenName = "Visitor",
                FamilyName = "NoAccess",
                EmailConfirmed = true,
                IsActive = true
            }
        };

        foreach (User user in users)
        {
            User? originalUser = await userManager.FindByEmailAsync(user.Email);

            if (originalUser is null)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, "StrongPassword123$");

                await userManager.CreateAsync(user);
            }

            if (user.Email.Equals("system.administrator@example.dev"))
            {
                await userManager.AddToRolesAsync(user, new[] { "Administrator", "Manager", "Default", "Visitor" });
            }

            if (user.Email.Equals("system.manager@example.dev"))
            {
                await userManager.AddToRolesAsync(user, new[] { "Manager", "Default", "Visitor" });
            }

            if (user.Email.Equals("default.access@example.dev"))
            {
                await userManager.AddToRolesAsync(user, new[] { "Default", "Visitor" });
            }

            if (user.Email.Equals("visitor.noaccess@example.dev"))
            {
                await userManager.AddToRolesAsync(user, new[] { "Visitor" });
            }
        }
    }
}
