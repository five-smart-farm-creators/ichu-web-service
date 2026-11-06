using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the readings of an animal captured within a half-open period, ordered by capture time.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
/// <param name="Period">The half-open period of the capture times.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of readings per page.</param>
public sealed record GetAnimalTelemetryQuery(Guid AnimalId, TimePeriod Period, int Page, int PageSize);
