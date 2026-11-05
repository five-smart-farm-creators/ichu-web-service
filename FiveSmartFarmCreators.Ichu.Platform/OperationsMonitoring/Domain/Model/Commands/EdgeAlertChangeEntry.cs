namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
/// A change recorded by the Edge Gateway for an existing alert: new evidence, a return to the permitted area,
/// or local attention registered by an authorized user.
/// </summary>
public sealed record EdgeAlertChangeEntry(
    Guid OperationId,
    Guid AlertId,
    string? Kind,
    Guid? DeviceId,
    IReadOnlyList<Guid> ReadingIds,
    DateTimeOffset? OccurredAt,
    Guid? AuthorId,
    string? Finding,
    string? Action,
    int? ExpectedVersion);
