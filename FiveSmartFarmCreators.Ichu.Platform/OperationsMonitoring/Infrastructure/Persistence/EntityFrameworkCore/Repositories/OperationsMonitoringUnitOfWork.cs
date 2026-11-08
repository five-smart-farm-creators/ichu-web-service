using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Saves the monitoring changes through the shared database context. A conflicting save is reported as a result, so the application decides per entry whether to continue.
/// </summary>
public class OperationsMonitoringUnitOfWork(AppDbContext context, ILogger<OperationsMonitoringUnitOfWork> logger)
    : IOperationsMonitoringUnitOfWork
{
    private const string ModelNamespace = "FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model";

    /// <inheritdoc />
    public async Task<bool> TryCompleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "The monitoring changes could not be saved.");
            DiscardChanges();
            return false;
        }
    }

    // Only monitoring entities are detached, so pending work of other modules in the shared context is untouched.
    private void DiscardChanges()
    {
        var entries = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity.GetType().Namespace?.StartsWith(ModelNamespace, StringComparison.Ordinal) == true)
            .ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
