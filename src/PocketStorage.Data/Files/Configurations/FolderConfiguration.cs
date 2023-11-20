using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Common.Configurations;
using PocketStorage.Domain.Files.Models;

namespace PocketStorage.Data.Files.Configurations;

public class FolderConfiguration : ChangeTrackingEntityConfiguration<Folder>
{
    public override void Configure(EntityTypeBuilder<Folder> builder)
    {
        base.Configure(builder);

        builder.ToTable(Tables.Folders, Schemas.Files);

        builder.HasKey(entity => entity.Id);

        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => entity.ParentId);
        builder.HasIndex(entity => entity.CategoryId);

        builder.Property(entity => entity.Name)
            .IsUnicode()
            .HasMaxLength(256);

        builder.HasMany(entity => entity.Children)
            .WithOne(entity => entity.Parent)
            .HasForeignKey(entity => entity.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(entity => entity.Files)
            .WithOne(entity => entity.Folder)
            .HasForeignKey(entity => entity.FolderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
