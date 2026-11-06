namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Describes the quality of a metric captured by a device.
/// </summary>
public enum MeasurementStatus
{
    /// <summary>The device captured a finite value.</summary>
    Valid,

    /// <summary>The device marked the capture as invalid and gave a reason.</summary>
    Invalid,

    /// <summary>The device sent nothing for the metric.</summary>
    Absent
}
