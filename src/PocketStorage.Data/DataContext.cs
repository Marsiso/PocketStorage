using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.EntityFrameworkCore.Models;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Data;

public class DataContext : IdentityDbContext<User, Role, string>
{
    private readonly ISaveChangesInterceptor _interceptor;

    public DataContext(DbContextOptions<DataContext> options, ISaveChangesInterceptor interceptor) : base(options)
    {
        _interceptor = interceptor;
    }

    public new DbSet<User> Users { get; set; } = default!;
    public new DbSet<Role> Roles { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.AspNetCore);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken>().ToTable(Tables.OpenIddictTokens, Schemas.OpenIddict);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication>().ToTable(Tables.OpenIddictApplications, Schemas.OpenIddict);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization>().ToTable(Tables.OpenIddictAuthorizations, Schemas.OpenIddict);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope>().ToTable(Tables.OpenIddictScopes, Schemas.OpenIddict);

        modelBuilder.Entity<User>().ToTable(Tables.AspNetCoreUsers, Schemas.AspNetCore);
        modelBuilder.Entity<Role>().ToTable(Tables.AspNetCoreRoles, Schemas.AspNetCore);
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable(Tables.AspNetCoreUserRoles, Schemas.AspNetCore);
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable(Tables.AspNetCoreUserClaims, Schemas.AspNetCore);
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable(Tables.AspNetCoreRoleClaims, Schemas.AspNetCore);
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable(Tables.AspNetCoreUserTokens, Schemas.AspNetCore);
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable(Tables.AspNetCoreUserLogins, Schemas.AspNetCore);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
