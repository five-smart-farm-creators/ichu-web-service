namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record TelemetryHistoryResource(
    Guid AnimalId,
    DateTimeOffset From,
    DateTimeOffset To,
    DateTimeOffset? OldestRetainedRecordedAt,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TelemetryReadingResource> Items);
