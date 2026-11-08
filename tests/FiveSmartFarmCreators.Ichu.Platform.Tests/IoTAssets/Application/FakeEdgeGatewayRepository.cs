using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public sealed class FakeEdgeGatewayRepository : IEdgeGatewayRepository
{
    public List<EdgeGateway> Items { get; } = [];

    public Task AddAsync(EdgeGateway gateway, CancellationToken cancellationToken)
    {
        Items.Add(gateway);
        return Task.CompletedTask;
    }

    public Task<EdgeGateway?> FindByIdAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(gateway => gateway.RanchId == ranchId && gateway.Id == gatewayId));

    public Task<EdgeGateway?> FindByIdAsNoTrackingAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken) =>
        FindByIdAsync(ranchId, gatewayId, cancellationToken);

    public Task<EdgeGateway?> FindByIdAsNoTrackingAsync(EdgeGatewayId gatewayId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(gateway => gateway.Id == gatewayId));

    public Task<IReadOnlyList<EdgeGateway>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<EdgeGateway>>(Items.Where(gateway => gateway.RanchId == ranchId).ToList());
}
