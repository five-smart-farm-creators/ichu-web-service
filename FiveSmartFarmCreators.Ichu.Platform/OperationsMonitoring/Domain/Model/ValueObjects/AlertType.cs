namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Identifies the condition that raised a monitoring alert.
/// </summary>
public enum AlertType
{
    /// <summary>Body temperature outside its configured range.</summary>
    Temperature,

    /// <summary>Activity index outside its configured range.</summary>
    Activity,

    /// <summary>The animal left its permitted grazing area.</summary>
    Geofence
}
