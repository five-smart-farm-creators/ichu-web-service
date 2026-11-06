using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;

public class WaterTroughCommandService(
    IWaterTroughRepository waterTroughRepository,
    IIoTAssetsAccessService accessService,
    ICattleLookupPort cattleLookup,
    IIoTAssetsTransaction transaction,
    TimeProvider timeProvider) : IWaterTroughCommandService
{
    public async Task<Result<WaterTrough>> HandleAsync(RegisterWaterTroughCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty)
        {
            return Result<WaterTrough>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<WaterTrough>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var waterTrough = new WaterTrough(command, timeProvider.GetUtcNow());
            if (command.LotId is Guid lotId)
            {
                var lot = await cattleLookup.LotExistsInRanchAsync(command.RanchId, lotId, token);
                if (lot.IsFailure)
                {
                    return Result<WaterTrough>.Failure(lot.DomainError);
                }

                if (!lot.Value)
                {
                    return Result<WaterTrough>.Failure(IoTAssetsErrors.TargetNotFound);
                }
            }

            await waterTroughRepository.AddAsync(waterTrough, token);
            return Result<WaterTrough>.Success(waterTrough);
        }, cancellationToken);
    }

    public async Task<Result<WaterTrough>> HandleAsync(ConfigureWaterTroughTemperatureCommand command,
        CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty || command.WaterTroughId == Guid.Empty || command.ExpectedVersion == Guid.Empty)
        {
            return Result<WaterTrough>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<WaterTrough>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var waterTrough = await waterTroughRepository.FindByIdAsync(command.RanchId, command.WaterTroughId, token);
            if (waterTrough is null)
            {
                return Result<WaterTrough>.Failure(IoTAssetsErrors.WaterTroughNotFound);
            }

            waterTrough.ConfigureTemperatureRange(new TemperatureRange(command.MinimumCelsius, command.MaximumCelsius),
                command.ExpectedVersion, timeProvider.GetUtcNow());
            return Result<WaterTrough>.Success(waterTrough);
        }, cancellationToken);
    }
}
