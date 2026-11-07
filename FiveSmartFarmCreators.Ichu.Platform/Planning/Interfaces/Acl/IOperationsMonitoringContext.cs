namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Resolves clinical intervention references through OperationsMonitoring.
/// </summary>
public interface IOperationsMonitoringContext
{
    /// <summary>
    /// Finds an intervention and confirms the animal it belongs to.
    /// </summary>
    /// <param name="interventionId">The clinical intervention identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The intervention reference, or null when no matching intervention exists.</returns>
    Task<ClinicalInterventionReference?> FindInterventionAsync(
        Guid interventionId,
        CancellationToken cancellationToken);
}
