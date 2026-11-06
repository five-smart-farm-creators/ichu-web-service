using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the alerts of a ranch, newest event first.
/// </summary>
/// <param name="RanchId">The ranch that owns the alerts.</param>
/// <param name="Status">Only alerts with this status, when given.</param>
/// <param name="AnimalId">Only alerts of this animal, when given.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of alerts per page.</param>
public record GetAlertsQuery(Guid RanchId, AlertStatus? Status, Guid? AnimalId, int Page, int PageSize);
