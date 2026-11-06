using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the configure threshold command from its REST resource.
/// </summary>
public static class ConfigureThresholdCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="ConfigureThresholdResource"/> into a <see cref="ConfigureThresholdCommand"/>.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the threshold definition.</param>
    /// <returns>The command with validated value objects.</returns>
    /// <exception cref="ArgumentException">Thrown when a value is missing, unknown or violates a value object rule.</exception>
    public static ConfigureThresholdCommand ToCommandFromResource(ConfigureThresholdResource resource)
    {
        if (resource.MinValue is not { } min || resource.MaxValue is not { } max)
        {
            throw new ArgumentException("Both the minimum and maximum values are required.");
        }

        var metricName = Enum.GetNames<MonitoredMetric>().FirstOrDefault(n => n.Equals(resource.Metric?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (metricName is null)
        {
            throw new ArgumentException($"The metric must be one of: {string.Join(", ", Enum.GetNames<MonitoredMetric>())}.");
        }

        var severityName = Enum.GetNames<AlertSeverity>().FirstOrDefault(n => n.Equals(resource.Severity?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (severityName is null)
        {
            throw new ArgumentException($"The severity must be one of: {string.Join(", ", Enum.GetNames<AlertSeverity>())}.");
        }

        return new ConfigureThresholdCommand(
            resource.RanchId,
            resource.LifeStage ?? string.Empty,
            Enum.Parse<MonitoredMetric>(metricName),
            new ThresholdRange(min, max),
            Enum.Parse<AlertSeverity>(severityName));
    }
}
