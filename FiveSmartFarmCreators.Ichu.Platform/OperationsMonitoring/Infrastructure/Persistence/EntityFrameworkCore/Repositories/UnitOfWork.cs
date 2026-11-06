using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Unit of work implementation that commits the monitoring changes through the shared database context.
/// </summary>
/// <param name="context">The shared database context.</param>
public class UnitOfWork(DbContext context) : IUnitOfWork
{
    private const string ModelNamespace = "FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model";

    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    // Only monitoring entities are detached, so pending work of other modules in the shared context is untouched.
    /// <inheritdoc />
    public void DiscardChanges()
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().Namespace?.StartsWith(ModelNamespace, StringComparison.Ordinal) == true)
            .ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
