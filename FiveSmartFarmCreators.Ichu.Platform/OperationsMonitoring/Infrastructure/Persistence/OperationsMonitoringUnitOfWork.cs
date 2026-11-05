using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class OperationsMonitoringUnitOfWork(DbContext context) : IOperationsMonitoringUnitOfWork
{
    private const string ModelNamespace = "FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model";

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw MonitoringException.Conflict("The record changed concurrently. Reload it before retrying.");
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw MonitoringException.Conflict("A record with the same identity already exists.");
        }
    }

    // Only monitoring entities are detached, so pending work of other modules in the shared context is untouched.
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
