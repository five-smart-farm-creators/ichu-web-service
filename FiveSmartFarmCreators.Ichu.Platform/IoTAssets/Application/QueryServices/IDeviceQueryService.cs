using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;

public interface IDeviceQueryService
{
    Task<Result<IoTDevice>> HandleAsync(GetDeviceByIdQuery query, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<IoTDevice>>> HandleAsync(GetDeviceInventoryQuery query, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<DeviceAssignment>>> HandleAsync(GetDeviceAssignmentsQuery query, CancellationToken cancellationToken);
}
