using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class GetAlertsQueryFromRequestAssembler
{
    public static Result<GetAlertsQuery> ToQueryFromRequest(Guid ranchId, string? status, Guid? cattleId, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Result<GetAlertsQuery>.Success(new GetAlertsQuery(ranchId, null, cattleId, page, pageSize));
        }

        return MonitoringEnumParser.Parse<AlertStatus>(status) is { } parsed
            ? Result<GetAlertsQuery>.Success(new GetAlertsQuery(ranchId, parsed, cattleId, page, pageSize))
            : Result<GetAlertsQuery>.Failure(OperationsMonitoringErrors.AlertStatusUnknown);
    }
}
