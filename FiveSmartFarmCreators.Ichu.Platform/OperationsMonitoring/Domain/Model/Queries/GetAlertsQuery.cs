using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

public sealed record GetAlertsQuery(Guid RanchId, AlertStatus? Status, Guid? AnimalId, int Page, int PageSize);
