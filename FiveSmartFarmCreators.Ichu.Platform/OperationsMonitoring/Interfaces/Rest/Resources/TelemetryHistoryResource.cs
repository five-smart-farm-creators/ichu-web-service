namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the readings of an animal within a half-open period.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
/// <param name="From">The first instant of the period.</param>
/// <param name="To">The instant that ends the period, excluded from it.</param>
/// <param name="OldestRetainedRecordedAt">The capture time of the oldest stored reading of the animal.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of readings per page.</param>
/// <param name="TotalCount">The total number of readings in the period.</param>
/// <param name="Items">The readings of the page.</param>
public record TelemetryHistoryResource(
    Guid AnimalId,
    DateTimeOffset From,
    DateTimeOffset To,
    DateTimeOffset? OldestRetainedRecordedAt,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<TelemetryReadingResource> Items);
