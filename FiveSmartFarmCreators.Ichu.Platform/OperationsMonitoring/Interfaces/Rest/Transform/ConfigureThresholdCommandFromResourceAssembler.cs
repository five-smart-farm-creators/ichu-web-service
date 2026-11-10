using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class ConfigureThresholdCommandFromResourceAssembler
{
    public static Result<ConfigureThresholdCommand> ToCommandFromResource(Guid ranchId, ConfigureThresholdResource resource)
    {
        if (resource.MinValue is not { } min || resource.MaxValue is not { } max)
        {
            return Result<ConfigureThresholdCommand>.Failure(OperationsMonitoringErrors.ThresholdBoundsRequired);
        }

        if (MonitoringEnumParser.Parse<MonitoredMetric>(resource.Metric) is not { } metric)
        {
            return Result<ConfigureThresholdCommand>.Failure(OperationsMonitoringErrors.MetricUnknown);
        }

        if (MonitoringEnumParser.Parse<AlertSeverity>(resource.Severity) is not { } severity)
        {
            return Result<ConfigureThresholdCommand>.Failure(OperationsMonitoringErrors.SeverityUnknown);
        }

        var range = ThresholdRange.Create(min, max);
        return range.IsFailure
            ? Result<ConfigureThresholdCommand>.Failure(range.DomainError)
            : Result<ConfigureThresholdCommand>.Success(new ConfigureThresholdCommand(
                ranchId, resource.LifeStage ?? string.Empty, metric, range.Value!, severity));
    }
}
