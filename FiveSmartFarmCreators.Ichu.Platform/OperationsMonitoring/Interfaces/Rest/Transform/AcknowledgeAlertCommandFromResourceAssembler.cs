using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class AcknowledgeAlertCommandFromResourceAssembler
{
    public static Result<AcknowledgeAlertCommand> ToCommandFromResource(Guid alertId, AcknowledgeAlertResource resource)
    {
        if (MonitoringEnumParser.Parse<AlertStatus>(resource.Status) is not { } status)
        {
            return Result<AcknowledgeAlertCommand>.Failure(OperationsMonitoringErrors.AlertStatusUnknown);
        }

        if (status != AlertStatus.Acknowledged)
        {
            return Result<AcknowledgeAlertCommand>.Failure(OperationsMonitoringErrors.AcknowledgedStatusOnly);
        }

        if (resource.ExpectedVersion is not { } expectedVersion)
        {
            return Result<AcknowledgeAlertCommand>.Failure(OperationsMonitoringErrors.ExpectedAlertVersionRequired);
        }

        return Result<AcknowledgeAlertCommand>.Success(new AcknowledgeAlertCommand(
            new MonitoringAlertId(alertId), resource.OperationId, resource.Finding ?? string.Empty, resource.Action, expectedVersion));
    }
}
