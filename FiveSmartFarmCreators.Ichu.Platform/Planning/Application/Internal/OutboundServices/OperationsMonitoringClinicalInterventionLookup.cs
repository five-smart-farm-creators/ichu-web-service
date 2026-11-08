using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Resolves clinical interventions through the Operations and Monitoring facade, so that field events never satisfy a clinical reference.
/// </summary>
public sealed class OperationsMonitoringClinicalInterventionLookup(IOperationsMonitoringContextFacade operationsMonitoring)
    : IClinicalInterventionLookup
{
    /// <inheritdoc />
    public async Task<ClinicalInterventionSnapshot?> FindAsync(Guid interventionId, CancellationToken cancellationToken)
    {
        var intervention = await operationsMonitoring.FindClinicalInterventionAsync(interventionId, cancellationToken);
        return intervention is null
            ? null
            : new ClinicalInterventionSnapshot(intervention.OperationId, intervention.RanchId, intervention.CattleId);
    }
}
