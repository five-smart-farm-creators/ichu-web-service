using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record TelemetryHistoryResource(
    Guid AnimalId,
    DateTimeOffset From,
    DateTimeOffset To,
    DateTimeOffset? OldestRetainedRecordedAt,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TelemetryReadingResource> Items)
{
    public static TelemetryHistoryResource FromHistory(TelemetryHistory history) =>
        new(
            history.AnimalId,
            history.Period.From,
            history.Period.To,
            history.OldestRetainedRecordedAt,
            history.Readings.Page,
            history.Readings.PageSize,
            history.Readings.TotalCount,
            history.Readings.Items.Select(TelemetryReadingResource.From).ToList());
}
