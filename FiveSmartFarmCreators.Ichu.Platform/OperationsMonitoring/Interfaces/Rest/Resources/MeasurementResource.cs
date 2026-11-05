using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>A captured metric. Status is Valid, Invalid or Absent; only a valid measurement carries a value.</summary>
public sealed record MeasurementResource(double? Value, string Unit, string Status, string? InvalidReason)
{
    public static MeasurementResource From(Measurement measurement, string unit) =>
        new(measurement.Value, unit, measurement.Status.ToString(), measurement.InvalidReason);
}
