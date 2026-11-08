using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     The oldest retained capture lets callers detect truncated history.
/// </summary>
public record TelemetryHistory(
    Guid AnimalId,
    TimePeriod Period,
    DateTimeOffset? OldestRetainedRecordedAt,
    PagedResult<TelemetryReading> Readings);
