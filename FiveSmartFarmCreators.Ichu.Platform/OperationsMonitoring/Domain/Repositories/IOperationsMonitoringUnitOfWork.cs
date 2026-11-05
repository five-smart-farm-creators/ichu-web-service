namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IOperationsMonitoringUnitOfWork
{
    /// <summary>Persists pending changes; concurrent or duplicate writes surface as a conflict.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Discards pending changes so that one rejected batch entry does not affect the next one.</summary>
    void DiscardChanges();
}
