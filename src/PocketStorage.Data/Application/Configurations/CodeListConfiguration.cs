using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Data.Configurations;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Data.Application.Configurations;

public class CodeListConfiguration : ChangeTrackingEntityConfiguration<CodeList>
{
    public override void Configure(EntityTypeBuilder<CodeList> builder)
    {
        base.Configure(builder);

        builder.ToTable(Tables.CodeLists, Schemas.Application);

        builder.HasKey(entity => entity.CodeListId);

        builder.HasIndex(entity => entity.Name)
            .IsUnique();

        builder.Property(entity => entity.Name)
            .IsUnicode()
            .HasMaxLength(256);

        builder.HasMany(entity => entity.CodeListItems)
            .WithOne()
            .HasForeignKey(entity => entity.CodeListId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}