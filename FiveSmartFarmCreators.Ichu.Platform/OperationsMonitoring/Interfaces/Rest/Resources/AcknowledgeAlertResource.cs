using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>Attention of a pending alert. Status must be Acknowledged; the version is the one last read by the client.</summary>
public sealed record AcknowledgeAlertResource(string? Status, string? Finding, string? Action, Guid OperationId, int? ExpectedVersion)
{
    public AcknowledgeAlertCommand ToCommand(Guid alertId)
    {
        if (MonitoringValueParser.ParseEnum<AlertStatus>(Status, "status") != AlertStatus.Acknowledged)
        {
            throw MonitoringException.InvalidInput("Only the Acknowledged status can be requested.");
        }

        var expectedVersion = ExpectedVersion ?? throw MonitoringException.InvalidInput("The expected alert version is required.");
        return new AcknowledgeAlertCommand(alertId, OperationId, Finding ?? string.Empty, Action, expectedVersion);
    }
}
