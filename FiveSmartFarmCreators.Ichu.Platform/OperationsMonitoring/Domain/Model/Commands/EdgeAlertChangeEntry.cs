using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A change recorded by an Edge Gateway for an existing alert: new evidence, a return to the permitted area, or attention registered locally by an authorized user. The kind is null when the gateway sent a missing or unknown value.
/// </summary>
public record EdgeAlertChangeEntry(
    Guid OperationId,
    MonitoringAlertId AlertId,
    AlertChangeKind? Kind,
    Guid? DeviceId,
    IReadOnlyList<Guid> ReadingIds,
    DateTimeOffset? OccurredAt,
    Guid? AuthorId,
    string? Finding,
    string? Action,
    int? ExpectedVersion);
