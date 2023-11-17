using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Common.Configurations;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Data.Application.Configurations;

public class RoleConfiguration : ChangeTrackingEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.HasMany(entity => entity.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
