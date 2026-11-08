using Cortex.Mediator;
using Cortex.Mediator.DependencyInjection;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.EventHandlers;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Shared.Infrastructure.Mediator;

public class DomainEventDispatchTests
{
    [Fact]
    public async Task PublishReachesTheHandlerOfTheRuntimeEventTypeWhenTheEventIsHeldAsIEvent()
    {
        var commands = new RecordingDeviceCommandService();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDeviceCommandService>(commands);
        services.AddCortexMediator([typeof(CattleDeactivatedEventHandler)]);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        IEvent domainEvent = new CattleDeactivatedEvent(Guid.NewGuid(), new CattleId(Guid.NewGuid()), ExitReason.Sale,
            new DateOnly(2026, 11, 7), new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));
        await DomainEventPublisher.PublishAsync(mediator, domainEvent, CancellationToken.None);

        Assert.Single(commands.ReleaseCommands);
    }
}
