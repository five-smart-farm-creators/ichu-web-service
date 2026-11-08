namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Status must be Acknowledged; the version is the one last read by the client.
/// </summary>
public record AcknowledgeAlertResource(string? Status, string? Finding, string? Action, Guid OperationId, int? ExpectedVersion);
