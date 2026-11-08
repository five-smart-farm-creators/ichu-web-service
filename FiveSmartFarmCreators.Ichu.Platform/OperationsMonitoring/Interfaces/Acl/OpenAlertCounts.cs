namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     Counts the alerts of a ranch that are not cleared, by severity, for attention priorities.
/// </summary>
public record OpenAlertCounts(int Critical, int Warning, int Informative);
