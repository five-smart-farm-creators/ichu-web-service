using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>Either a value or the reason the device marked the capture as invalid.</summary>
public sealed record CapturedMetricResource(double? Value, string? InvalidReason)
{
    public CapturedMetric ToCapturedMetric() => new(Value, InvalidReason);
}
