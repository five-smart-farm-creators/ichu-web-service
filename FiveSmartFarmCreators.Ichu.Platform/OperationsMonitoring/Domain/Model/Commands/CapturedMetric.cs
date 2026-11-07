namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A metric as transmitted by an Edge Gateway, validated when its reading is processed.
/// </summary>
/// <param name="Value">The captured value, if any.</param>
/// <param name="InvalidReason">The reason the device marked the capture as invalid, if it did.</param>
public record CapturedMetric(double? Value, string? InvalidReason);
