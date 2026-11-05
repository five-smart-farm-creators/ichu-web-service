using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>Readings of an animal for a period, with the oldest retained capture so callers can detect truncated history.</summary>
public sealed record TelemetryHistory(
    Guid AnimalId,
    TimePeriod Period,
    DateTimeOffset? OldestRetainedRecordedAt,
    PagedResult<TelemetryReading> Readings);
