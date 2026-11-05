namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public static class MonitoringUnits
{
    public const string Celsius = "celsius";
    public const string Index = "index";
    public const string Minutes = "minutes";
    public const string Percent = "percent";

    // Every reading reaches the central API through an Edge Gateway; collars never call the API directly.
    public const string EdgeGatewayOrigin = "EdgeGateway";
}
