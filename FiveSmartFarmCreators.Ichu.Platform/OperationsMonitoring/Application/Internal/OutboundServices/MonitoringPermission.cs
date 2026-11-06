namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Identifies a ranch permission that monitoring operations require.
/// </summary>
public enum MonitoringPermission
{
    /// <summary>Read the readings, alerts, rules and operations of the ranch.</summary>
    ReadMonitoring,

    /// <summary>Record attention for an alert.</summary>
    AttendAlerts,

    /// <summary>Register field events.</summary>
    RecordFieldEvents,

    /// <summary>Register clinical interventions.</summary>
    RecordClinicalInterventions,

    /// <summary>Configure thresholds and geofences.</summary>
    ManageRules
}
