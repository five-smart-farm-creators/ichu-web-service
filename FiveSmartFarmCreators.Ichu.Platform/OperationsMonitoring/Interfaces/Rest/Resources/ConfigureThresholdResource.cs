using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>Configures the normal range of BodyTemperature (celsius) or ActivityIndex (index) for one life stage.</summary>
public sealed record ConfigureThresholdResource(
    Guid RanchId,
    string? LifeStage,
    string? Metric,
    double? MinValue,
    double? MaxValue,
    string? Severity)
{
    public ConfigureThresholdCommand ToCommand()
    {
        if (MinValue is not { } min || MaxValue is not { } max)
        {
            throw MonitoringException.InvalidInput("Both the minimum and maximum values are required.");
        }

        return new ConfigureThresholdCommand(
            RanchId,
            LifeStage ?? string.Empty,
            MonitoringValueParser.ParseEnum<MonitoredMetric>(Metric, "metric"),
            new ThresholdRange(min, max),
            MonitoringValueParser.ParseEnum<AlertSeverity>(Severity, "severity"));
    }
}
