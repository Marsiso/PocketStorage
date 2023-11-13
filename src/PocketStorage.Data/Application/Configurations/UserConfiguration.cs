using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Common.Configurations;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Data.Application.Configurations;

public class UserConfiguration : ChangeTrackingEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.GivenName)
            .IsUnicode()
            .HasMaxLength(256);

        builder.Property(entity => entity.FamilyName)
            .IsUnicode()
            .HasMaxLength(256);

        builder.Property(entity => entity.ProfilePhoto)
            .HasMaxLength(4096);

        builder.HasMany(entity => entity.Folders)
            .WithOne(entity => entity.User)
            .HasForeignKey(entity => entity.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
