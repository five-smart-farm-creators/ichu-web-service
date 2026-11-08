using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Lists the alerts of a ranch, newest event first.
/// </summary>
public record GetAlertsQuery(Guid RanchId, AlertStatus? Status, Guid? AnimalId, int Page, int PageSize);
