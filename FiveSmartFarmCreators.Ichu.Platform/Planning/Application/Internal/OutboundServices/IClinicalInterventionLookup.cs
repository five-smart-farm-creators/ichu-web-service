namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Anti-corruption port towards Operations and Monitoring, which resolves only clinical interventions.
/// </summary>
public interface IClinicalInterventionLookup
{
    Task<ClinicalInterventionSnapshot?> FindAsync(Guid interventionId, CancellationToken cancellationToken);
}
