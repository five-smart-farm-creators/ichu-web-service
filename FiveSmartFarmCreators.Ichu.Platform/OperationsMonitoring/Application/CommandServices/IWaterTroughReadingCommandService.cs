using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface IWaterTroughReadingCommandService
{
    Task<Result<WaterTroughReadingReceipt>> Handle(Guid controllerId, RegisterWaterControllerReadingCommand command,
        CancellationToken cancellationToken);
}
