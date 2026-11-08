using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface ITelemetryReadingQueryService
{
    Task<Result<TelemetryHistory>> Handle(GetAnimalTelemetryQuery query, CancellationToken cancellationToken);

    Task<Result<LatestTelemetry>> Handle(GetLatestAnimalTelemetryQuery query, CancellationToken cancellationToken);
}
