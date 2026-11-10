using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Lists the livestock operations of an animal within a half-open period.
/// </summary>
public record GetLivestockOperationsQuery(Guid RanchId, Guid AnimalId, TimePeriod Period);
