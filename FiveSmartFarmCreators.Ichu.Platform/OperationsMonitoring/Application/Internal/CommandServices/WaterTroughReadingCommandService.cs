using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

public sealed class WaterTroughReadingCommandService(
    IEdgeGatewayAuthenticator gatewayAuthenticator,
    IIoTAssetsContextFacade ioTAssets,
    IWaterTroughReadingRepository readings,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IWaterTroughReadingCommandService
{
    public async Task<Result<WaterTroughReadingReceipt>> Handle(Guid controllerId,
        RegisterWaterControllerReadingCommand request, CancellationToken cancellationToken)
    {
        if (controllerId == Guid.Empty || request.ControllerId != controllerId || request.ReadingId == Guid.Empty
            || request.CapturedAt == default || request.CapturedAt.UtcTicks % 10 != 0
            || !Enum.IsDefined(request.TemperatureUnit) || !Enum.IsDefined(request.HeaterState))
        {
            return Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.WaterReadingInvalid);
        }

        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.GatewayUnauthenticated);
        }

        var receivedAt = clock.GetUtcNow();
        if (request.CapturedAt.ToUniversalTime() > receivedAt + options.Value.ClockSkewTolerance)
        {
            return Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.CaptureTimeAhead);
        }

        var assignment = await ioTAssets.FindWaterControllerAssignmentAsync(controllerId, request.CapturedAt,
            cancellationToken);
        if (assignment is null || !assignment.IsWaterController)
        {
            return Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.WaterControllerNotFound);
        }

        if (assignment.RanchId != gateway.RanchId || assignment.WaterTroughId is not { } waterTroughId)
        {
            return Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.WaterControllerGatewayAccessDenied);
        }

        var command = request with
        {
            GatewayId = gateway.GatewayId,
            RanchId = gateway.RanchId,
            ControllerId = controllerId,
            WaterTroughId = waterTroughId,
            ReceivedAt = receivedAt
        };

        var existing = await readings.FindByControllerAndReadingIdAsync(controllerId, request.ReadingId,
            cancellationToken);
        if (existing is not null)
        {
            return existing.RanchId == gateway.RanchId && existing.HasSameObservation(command)
                ? Result<WaterTroughReadingReceipt>.Success(new WaterTroughReadingReceipt(existing, false))
                : Result<WaterTroughReadingReceipt>.Failure(OperationsMonitoringErrors.WaterReadingIdentifierConflict);
        }

        var readingResult = WaterTroughReading.Create(command);
        if (readingResult.IsFailure)
        {
            return Result<WaterTroughReadingReceipt>.Failure(readingResult.DomainError);
        }

        await readings.AddAsync(readingResult.Value!, cancellationToken);
        if (await unitOfWork.TryCompleteAsync(cancellationToken))
        {
            return Result<WaterTroughReadingReceipt>.Success(new WaterTroughReadingReceipt(readingResult.Value!, true));
        }

        // A concurrent identical request may have won the unique (controller, readingId) key.
        var concurrent = await readings.FindByControllerAndReadingIdAsync(controllerId, request.ReadingId,
            cancellationToken);
        return concurrent is not null && concurrent.RanchId == gateway.RanchId && concurrent.HasSameObservation(command)
            ? Result<WaterTroughReadingReceipt>.Success(new WaterTroughReadingReceipt(concurrent, false))
            : Result<WaterTroughReadingReceipt>.Failure(concurrent is null
                ? OperationsMonitoringErrors.WaterReadingConcurrentlyStored
                : OperationsMonitoringErrors.WaterReadingIdentifierConflict);
    }
}
