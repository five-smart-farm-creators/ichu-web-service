using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Read model of the readings of an animal for a period, with the oldest retained capture so that
///     callers can detect truncated history.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
/// <param name="Period">The requested half-open period.</param>
/// <param name="OldestRetainedRecordedAt">The capture time of the oldest stored reading of the animal.</param>
/// <param name="Readings">The requested page of readings.</param>
public record TelemetryHistory(
    Guid AnimalId,
    TimePeriod Period,
    DateTimeOffset? OldestRetainedRecordedAt,
    PagedResult<TelemetryReading> Readings);
