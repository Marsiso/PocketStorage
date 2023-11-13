using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Data.Common.Configurations;

public class ChangeTrackingEntityConfiguration<TEntity> : EntityBaseConfiguration<TEntity> where TEntity : class, IChangeTrackingEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(entity => entity.UserCreatedBy)
            .WithMany()
            .HasForeignKey(entity => entity.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.UserUpdatedBy)
            .WithMany()
            .HasForeignKey(entity => entity.UpdatedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
