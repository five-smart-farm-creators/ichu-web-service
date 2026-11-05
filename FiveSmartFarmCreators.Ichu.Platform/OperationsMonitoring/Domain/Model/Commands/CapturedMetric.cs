namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>A metric as transmitted by the Edge Gateway, validated when its reading is processed.</summary>
public sealed record CapturedMetric(double? Value, string? InvalidReason);
