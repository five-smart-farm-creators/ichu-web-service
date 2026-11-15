namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class MonitoringUnits
{
    public const string BeatsPerMinute = "bpm";

    public const string Celsius = "celsius";

    public const string Index = "index";

    public const string Minutes = "minutes";

    public const string Percent = "percent";

    /// <summary>
    ///     Collars reach the central API only through an Edge Gateway, so every reading and alert carries this origin.
    /// </summary>
    public const string EdgeGatewayOrigin = "EdgeGateway";
}
