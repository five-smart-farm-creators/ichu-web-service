namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a metric sent by an Edge Gateway: either a value or the reason the device marked the capture as invalid.
/// </summary>
/// <param name="Value">The captured value.</param>
/// <param name="InvalidReason">The reason the capture is invalid.</param>
public record CapturedMetricResource(double? Value, string? InvalidReason);
