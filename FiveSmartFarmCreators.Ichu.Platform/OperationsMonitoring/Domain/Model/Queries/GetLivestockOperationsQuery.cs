using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the livestock operations of an animal within a half-open period.
/// </summary>
/// <param name="AnimalId">The animal the operations were applied to.</param>
/// <param name="Period">The half-open period of the occurrence times.</param>
public sealed record GetLivestockOperationsQuery(Guid AnimalId, TimePeriod Period);
