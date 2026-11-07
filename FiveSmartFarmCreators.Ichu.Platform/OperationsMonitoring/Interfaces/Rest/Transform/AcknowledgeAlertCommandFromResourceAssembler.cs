using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the acknowledge alert command from its REST resource.
/// </summary>
public static class AcknowledgeAlertCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms an <see cref="AcknowledgeAlertResource"/> into an <see cref="AcknowledgeAlertCommand"/>.
    /// </summary>
    /// <param name="alertId">The alert identified by the route.</param>
    /// <param name="resource">The HTTP resource containing the attention data.</param>
    /// <returns>The command to attend the alert.</returns>
    /// <exception cref="ArgumentException">Thrown when the status is not Acknowledged or the expected version is missing.</exception>
    public static AcknowledgeAlertCommand ToCommandFromResource(Guid alertId, AcknowledgeAlertResource resource)
    {
        var name = Enum.GetNames<AlertStatus>().FirstOrDefault(n => n.Equals(resource.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (name is null)
        {
            throw new ArgumentException($"The status must be one of: {string.Join(", ", Enum.GetNames<AlertStatus>())}.");
        }

        if (Enum.Parse<AlertStatus>(name) != AlertStatus.Acknowledged)
        {
            throw new ArgumentException("Only the Acknowledged status can be requested.");
        }

        var expectedVersion = resource.ExpectedVersion ?? throw new ArgumentException("The expected alert version is required.");
        return new AcknowledgeAlertCommand(alertId, resource.OperationId, resource.Finding ?? string.Empty, resource.Action, expectedVersion);
    }
}
