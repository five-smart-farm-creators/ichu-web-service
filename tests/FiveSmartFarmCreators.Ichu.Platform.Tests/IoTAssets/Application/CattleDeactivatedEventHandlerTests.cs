using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.EventHandlers;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using Microsoft.Extensions.Logging.Abstractions;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public class CattleDeactivatedEventHandlerTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset DeactivatedAt = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ReleasesTheCollarOfTheDeactivatedAnimal()
    {
        var commands = new RecordingDeviceCommandService();
        var handler = new CattleDeactivatedEventHandler(commands, NullLogger<CattleDeactivatedEventHandler>.Instance);
        var domainEvent = new CattleDeactivatedEvent(RanchId, new CattleId(CattleId), ExitReason.Death,
            new DateOnly(2026, 11, 7), DeactivatedAt);

        await handler.Handle(domainEvent, CancellationToken.None);

        var command = Assert.Single(commands.ReleaseCommands);
        Assert.Equal(new ReleaseCollarForInactiveCattleCommand(RanchId, CattleId, DeactivatedAt), command);
    }
}
