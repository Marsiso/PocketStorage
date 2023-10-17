using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Configurations;
using File = PocketStorage.Domain.FileSystem.Models.File;

namespace PocketStorage.Data.FileSystem.Configurations;

public class FileConfiguration : ChangeTrackingEntityConfiguration<File>
{
    public override void Configure(EntityTypeBuilder<File> builder)
    {
        base.Configure(builder);

        builder.ToTable(Tables.Files, Schemas.FileSystem);

        builder.HasKey(entity => entity.Id);

        builder.HasIndex(entity => entity.FolderId);
        builder.HasIndex(entity => entity.UnsafeName);

        builder.Property(entity => entity.SafeName)
            .HasMaxLength(256);

        builder.Property(entity => entity.UnsafeName)
            .IsUnicode()
            .HasMaxLength(256);

        builder.Property(entity => entity.Location)
            .HasMaxLength(1024);

        builder.Property(entity => entity.Extension)
            .HasMaxLength(1024);

        builder.Property(entity => entity.MimeType)
            .HasMaxLength(1024);
    }
}