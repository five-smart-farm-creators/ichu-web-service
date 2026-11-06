namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a field of a telemetry reading.
/// </summary>
public enum TelemetryField
{
    /// <summary>Body temperature.</summary>
    BodyTemperature,

    /// <summary>Activity index.</summary>
    ActivityIndex,

    /// <summary>Rumination minutes.</summary>
    RuminationMinutes,

    /// <summary>Battery level of the device.</summary>
    BatteryPercent,

    /// <summary>GPS position.</summary>
    Position
}
