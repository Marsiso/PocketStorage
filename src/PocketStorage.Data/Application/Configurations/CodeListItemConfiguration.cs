using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Configurations;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Data.Application.Configurations;

public class CodeListItemConfiguration : ChangeTrackingEntityConfiguration<CodeListItem>
{
    public override void Configure(EntityTypeBuilder<CodeListItem> builder)
    {
        base.Configure(builder);

        builder.ToTable(Tables.CodeListItems, Schemas.Application);

        builder.HasKey(entity => entity.CodeListItemId);

        builder.HasIndex(entity => entity.CodeListId);
        builder.HasIndex(entity => entity.Value);

        builder.Property(entity => entity.Value)
            .IsUnicode()
            .HasMaxLength(256);
    }
}