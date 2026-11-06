using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;

public interface IDeviceCommandService
{
    Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommand command, CancellationToken cancellationToken);
    Task<Result<IoTDevice>> HandleAsync(AssignDeviceCommand command, CancellationToken cancellationToken);
    Task<Result<IoTDevice>> HandleAsync(ReleaseDeviceCommand command, CancellationToken cancellationToken);
    Task<Result<IoTDevice>> HandleAsync(SetDeviceMaintenanceCommand command, CancellationToken cancellationToken);
    Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommunicationCommand command, CancellationToken cancellationToken);
    Task<Result<int>> HandleAsync(ReleaseCollarForInactiveCattleCommand command, CancellationToken cancellationToken);
}
