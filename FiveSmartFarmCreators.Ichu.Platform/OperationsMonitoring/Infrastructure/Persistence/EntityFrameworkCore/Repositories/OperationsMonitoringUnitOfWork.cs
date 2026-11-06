using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Unit of work that commits the monitoring changes through the shared database context and can discard them.
/// </summary>
public class OperationsMonitoringUnitOfWork : UnitOfWork, IOperationsMonitoringUnitOfWork
{
    private const string ModelNamespace = "FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model";

    private readonly AppDbContext _context;

    /// <summary>
    ///     Initializes the unit of work with the shared database context.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public OperationsMonitoringUnitOfWork(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public void DiscardChanges()
    {
        // Only monitoring entities are detached, so pending work of other modules in the shared context is untouched.
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().Namespace?.StartsWith(ModelNamespace, StringComparison.Ordinal) == true)
            .ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
