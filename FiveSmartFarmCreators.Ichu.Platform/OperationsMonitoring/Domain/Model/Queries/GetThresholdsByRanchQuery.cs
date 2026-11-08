namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Lists the thresholds configured by a ranch.
/// </summary>
public record GetThresholdsByRanchQuery(Guid RanchId);
