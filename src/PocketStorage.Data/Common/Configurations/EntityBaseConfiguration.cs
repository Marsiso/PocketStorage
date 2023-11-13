using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Data.Common.Configurations;

public class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IEntityBase
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(entity => entity.IsActive);

        builder.HasQueryFilter(entity => entity.IsActive);
    }
}
