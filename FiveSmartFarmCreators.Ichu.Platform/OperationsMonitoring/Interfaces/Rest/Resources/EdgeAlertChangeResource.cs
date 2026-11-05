using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>A change to a synchronized alert. Kind is Evidence, Cleared or Acknowledged.</summary>
public sealed record EdgeAlertChangeResource(
    Guid OperationId,
    Guid AlertId,
    string? Kind,
    Guid? DeviceId,
    IReadOnlyList<Guid>? ReadingIds,
    DateTimeOffset? OccurredAt,
    Guid? AuthorId,
    string? Finding,
    string? Action,
    int? ExpectedVersion)
{
    public EdgeAlertChangeEntry ToEntry() =>
        new(OperationId, AlertId, Kind, DeviceId, ReadingIds ?? [], OccurredAt, AuthorId, Finding, Action, ExpectedVersion);
}
