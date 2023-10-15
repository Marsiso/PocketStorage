using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Data.Interceptors;

public class AuditTrailInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not DataContext context)
        {
            return base.SavingChanges(eventData, result);
        }

        OnBeforeSavedChanges(context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DataContext context)
        {
            return new ValueTask<InterceptionResult<int>>(result);
        }

        OnBeforeSavedChanges(context);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not DataContext context)
        {
            return base.SavedChanges(eventData, result);
        }

        OnAfterSavedChanges(context);

        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DataContext context)
        {
            return new ValueTask<int>(result);
        }

        OnAfterSavedChanges(context);

        return new ValueTask<int>(result);
    }

    private static void OnBeforeSavedChanges(DataContext context)
    {
        context.ChangeTracker.DetectChanges();

        DateTime date = DateTime.UtcNow;

        foreach (EntityEntry<IChangeTrackingEntity> entityEntry in context.ChangeTracker.Entries<IChangeTrackingEntity>())
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    entityEntry.Entity.IsActive = true;
                    entityEntry.Entity.DateCreated = entityEntry.Entity.DateUpdated = date;
                    continue;

                case EntityState.Modified:
                    entityEntry.Entity.DateUpdated = date;
                    continue;

                case EntityState.Deleted:
                    throw new InvalidOperationException();

                default:
                    continue;
            }
        }
    }

    private static void OnAfterSavedChanges(DataContext context)
    {
    }
}
