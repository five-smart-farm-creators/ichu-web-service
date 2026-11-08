using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Internal.EventHandlers;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.EventHandlers;

/// <summary>
///     Frees the collar of an animal after its exit is saved, so that the device can serve another target. The exit
///     stays recorded even when the release fails, so a failure is logged for follow-up instead of rolled back.
/// </summary>
public class CattleDeactivatedEventHandler(
    IDeviceCommandService deviceCommandService,
    ILogger<CattleDeactivatedEventHandler> logger) : IEventHandler<CattleDeactivatedEvent>
{
    /// <inheritdoc />
    public async Task Handle(CattleDeactivatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var release = await deviceCommandService.HandleAsync(
            new ReleaseCollarForInactiveCattleCommand(domainEvent.RanchId, domainEvent.CattleId.Value, domainEvent.DeactivatedAt),
            cancellationToken);
        if (release.IsFailure)
        {
            logger.LogWarning("The collar of deactivated cattle {CattleId} was not released: {ErrorCode}",
                domainEvent.CattleId.Value, release.DomainError.Code);
        }
    }
}
