namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the thresholds configured by a ranch.
/// </summary>
/// <param name="RanchId">The ranch that configured the thresholds.</param>
public sealed record GetThresholdsByRanchQuery(Guid RanchId);
