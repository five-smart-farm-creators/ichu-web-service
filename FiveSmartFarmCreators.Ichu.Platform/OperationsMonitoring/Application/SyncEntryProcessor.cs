using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>Processes one synchronized entry in isolation and converts its failure into an individual result.</summary>
public sealed class SyncEntryProcessor(IOperationsMonitoringUnitOfWork unitOfWork)
{
    public async Task<SyncEntryResult> ProcessAsync(SyncEntryKind kind, Guid id, Func<Task<SyncEntryResult>> synchronize)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await synchronize();
            }
            catch (MonitoringException error) when (error.Kind == MonitoringErrorKind.Conflict && attempt == 1)
            {
                // A concurrent write of the same identifier is evaluated again against the stored record.
                unitOfWork.DiscardChanges();
            }
            catch (MonitoringException error) when (error.Kind != MonitoringErrorKind.Unauthenticated)
            {
                unitOfWork.DiscardChanges();
                var status = error.Kind == MonitoringErrorKind.Conflict ? SyncEntryStatus.Conflict : SyncEntryStatus.Rejected;
                return new SyncEntryResult(kind, id, status, error.Message);
            }
        }
    }
}
