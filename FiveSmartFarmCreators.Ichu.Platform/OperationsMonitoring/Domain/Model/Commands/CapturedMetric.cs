namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A metric as transmitted by an Edge Gateway, validated when its reading is processed.
/// </summary>
public record CapturedMetric(double? Value, string? InvalidReason);
