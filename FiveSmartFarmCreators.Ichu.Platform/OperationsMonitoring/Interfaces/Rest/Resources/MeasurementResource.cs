namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a captured metric. Status is Valid, Invalid or Absent; only a valid measurement carries a value.
/// </summary>
/// <param name="Value">The captured value.</param>
/// <param name="Unit">The unit of the value.</param>
/// <param name="Status">Valid, Invalid or Absent.</param>
/// <param name="InvalidReason">The reason the capture is invalid.</param>
public record MeasurementResource(double? Value, string Unit, string Status, string? InvalidReason);
