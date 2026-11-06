namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Units and origin labels reported by the monitoring resources.
/// </summary>
public static class MonitoringUnits
{
    /// <summary>Unit of body temperature.</summary>
    public const string Celsius = "celsius";

    /// <summary>Unit of the activity index.</summary>
    public const string Index = "index";

    /// <summary>Unit of rumination.</summary>
    public const string Minutes = "minutes";

    /// <summary>Unit of the battery level.</summary>
    public const string Percent = "percent";

    /// <summary>Origin of every reading and alert, because collars reach the central API only through an Edge Gateway.</summary>
    public const string EdgeGatewayOrigin = "EdgeGateway";
}
