using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class TimePeriodFromRequestAssembler
{
    public static Result<TimePeriod> ToPeriodFromRequest(DateTimeOffset? from, DateTimeOffset? to) =>
        from is { } start && to is { } end
            ? TimePeriod.Create(start, end)
            : Result<TimePeriod>.Failure(OperationsMonitoringErrors.PeriodRequired);
}
