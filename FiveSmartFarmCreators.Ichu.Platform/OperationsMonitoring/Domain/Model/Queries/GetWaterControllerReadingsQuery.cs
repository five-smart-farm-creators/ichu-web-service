using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

public sealed record GetWaterControllerReadingsQuery(Guid ControllerId, TimePeriod Period, int Page, int PageSize);
