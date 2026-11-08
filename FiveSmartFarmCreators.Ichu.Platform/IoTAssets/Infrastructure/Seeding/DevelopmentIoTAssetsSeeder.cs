using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Seeding;

/// <summary>
///     Registers the Development collars and edge gateway, and assigns two collars to the Development animals from the
///     seed instant. Records that already exist are skipped.
/// </summary>
public sealed class DevelopmentIoTAssetsSeeder(
    IIoTDeviceRepository devices,
    IEdgeGatewayRepository gateways,
    AppDbContext context,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    private static readonly DevelopmentCollar[] Collars =
    [
        new(new Guid("9d4a5f3b-3a6e-4f84-8daa-4e5f6a7b8c93"), "ICHU-DEV-0001", DevelopmentSeed.MilkingCowId),
        new(new Guid("ae5b6a4c-4b7f-4a95-9ebb-5f6a7b8c9da4"), "ICHU-DEV-0002", DevelopmentSeed.HeiferId),
        new(new Guid("bf6c7b5d-5c8a-4ba6-8fcc-6a7b8c9daeb5"), "ICHU-DEV-0003", null)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        foreach (var collar in Collars)
        {
            if (await devices.FindByIdAsNoTrackingAsync(new DeviceId(collar.Id), cancellationToken) is null)
            {
                await AddCollarAsync(collar, now, cancellationToken);
            }
        }

        if (await gateways.FindByIdAsNoTrackingAsync(new EdgeGatewayId(DevelopmentSeed.GatewayId), cancellationToken) is null)
        {
            await AddGatewayAsync(now, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task AddCollarAsync(DevelopmentCollar collar, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var registration = IoTDevice.Register(
            new RegisterDeviceCommand(DevelopmentSeed.RanchId, collar.Serial, DeviceType.SmartCollar, null), now);
        if (registration.IsFailure)
        {
            throw new InvalidOperationException(registration.DomainError.Message);
        }

        var device = registration.Value!;
        // Development-only: the edge's seed references these fixed identifiers.
        context.Entry(device).Property(entry => entry.Id).CurrentValue = new DeviceId(collar.Id);
        if (collar.CattleId is { } cattleId)
        {
            var assignment = device.AssignTo(cattleId, now);
            if (assignment.IsFailure)
            {
                throw new InvalidOperationException(assignment.DomainError.Message);
            }
        }

        await devices.AddAsync(device, cancellationToken);
    }

    private async Task AddGatewayAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var registration = EdgeGateway.Register(
            new RegisterEdgeGatewayCommand(DevelopmentSeed.RanchId, "Development gateway"),
            GatewayKeyHash.Of(DevelopmentSeed.GatewaySecret), now);
        if (registration.IsFailure)
        {
            throw new InvalidOperationException(registration.DomainError.Message);
        }

        var gateway = registration.Value!;
        context.Entry(gateway).Property(entry => entry.Id).CurrentValue = new EdgeGatewayId(DevelopmentSeed.GatewayId);
        await gateways.AddAsync(gateway, cancellationToken);
    }

    private sealed record DevelopmentCollar(Guid Id, string Serial, Guid? CattleId);
}
