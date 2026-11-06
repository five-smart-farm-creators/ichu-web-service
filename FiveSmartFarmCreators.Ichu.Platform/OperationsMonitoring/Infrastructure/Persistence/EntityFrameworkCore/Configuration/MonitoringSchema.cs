namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Database conventions shared by the monitoring entity configurations.
/// </summary>
public static class MonitoringSchema
{
    /// <summary>Name of the database schema that holds the monitoring tables.</summary>
    public const string Name = "monitoring";

    /// <summary>Maximum length of the columns that store enumeration names.</summary>
    public const int EnumLength = 32;
}
