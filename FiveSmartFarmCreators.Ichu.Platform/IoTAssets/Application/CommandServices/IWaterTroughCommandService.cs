using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;

public interface IWaterTroughCommandService
{
    Task<Result<WaterTrough>> HandleAsync(RegisterWaterTroughCommand command, CancellationToken cancellationToken);
    Task<Result<WaterTrough>> HandleAsync(ConfigureWaterTroughTemperatureCommand command, CancellationToken cancellationToken);
}
