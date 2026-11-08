namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Kind is Evidence, Cleared or Acknowledged.
/// </summary>
public record EdgeAlertChangeResource(
    Guid OperationId,
    Guid AlertId,
    string? Kind,
    Guid? DeviceId,
    IReadOnlyList<Guid>? ReadingIds,
    DateTimeOffset? OccurredAt,
    Guid? AuthorId,
    string? Finding,
    string? Action,
    int? ExpectedVersion);
