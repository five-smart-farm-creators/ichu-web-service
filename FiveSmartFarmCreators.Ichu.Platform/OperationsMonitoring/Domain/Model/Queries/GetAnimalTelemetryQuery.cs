using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Lists the readings of an animal captured within a half-open period, ordered by capture time.
/// </summary>
public record GetAnimalTelemetryQuery(Guid AnimalId, TimePeriod Period, int Page, int PageSize);
