using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.EntityFrameworkCore.Models;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Files.Models;
using File = PocketStorage.Domain.Files.Models.File;

namespace PocketStorage.Data;

public class DataContext : IdentityDbContext<User, Role, string>
{
    private readonly ISaveChangesInterceptor _interceptor;

    public DataContext(DbContextOptions<DataContext> options, ISaveChangesInterceptor interceptor) : base(options) => _interceptor = interceptor;

    public new DbSet<User> Users { get; set; } = default!;
    public new DbSet<Role> Roles { get; set; } = default!;
    public DbSet<CodeList> CodeLists { get; set; } = default!;
    public DbSet<CodeListItem> CodeListItems { get; set; } = default!;
    public DbSet<Folder> Folders { get; set; } = default!;
    public DbSet<File> Files { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.Files);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken>().ToTable(Tables.Tokens, Schemas.OpenId);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication>().ToTable(Tables.Applications, Schemas.OpenId);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization>().ToTable(Tables.Authorizations, Schemas.OpenId);
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope>().ToTable(Tables.Scopes, Schemas.OpenId);

        modelBuilder.Entity<User>().ToTable(Tables.Users, Schemas.Application);
        modelBuilder.Entity<Role>().ToTable(Tables.Roles, Schemas.Application);
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable(Tables.UserRoles, Schemas.Application);
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable(Tables.UserClaims, Schemas.Application);
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable(Tables.RoleClaims, Schemas.Application);
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable(Tables.UserTokens, Schemas.Application);
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable(Tables.UserLogins, Schemas.Application);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
