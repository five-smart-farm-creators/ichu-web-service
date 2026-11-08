namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Status is Valid, Invalid or Absent; only a valid measurement carries a value.
/// </summary>
public record MeasurementResource(double? Value, string Unit, string Status, string? InvalidReason);
