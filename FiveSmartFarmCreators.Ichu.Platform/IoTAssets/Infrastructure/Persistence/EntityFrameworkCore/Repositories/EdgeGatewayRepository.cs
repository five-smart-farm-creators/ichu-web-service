using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class EdgeGatewayRepository(AppDbContext context) : IEdgeGatewayRepository
{
    public async Task AddAsync(EdgeGateway gateway, CancellationToken cancellationToken)
    {
        await context.Set<EdgeGateway>().AddAsync(gateway, cancellationToken);
    }

    public Task<EdgeGateway?> FindByIdAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken)
    {
        return context.Set<EdgeGateway>()
            .FirstOrDefaultAsync(gateway => gateway.RanchId == ranchId && gateway.Id == gatewayId, cancellationToken);
    }

    public Task<EdgeGateway?> FindByIdAsNoTrackingAsync(Guid ranchId, EdgeGatewayId gatewayId, CancellationToken cancellationToken)
    {
        return context.Set<EdgeGateway>().AsNoTracking()
            .FirstOrDefaultAsync(gateway => gateway.RanchId == ranchId && gateway.Id == gatewayId, cancellationToken);
    }

    public Task<EdgeGateway?> FindByIdAsNoTrackingAsync(EdgeGatewayId gatewayId, CancellationToken cancellationToken)
    {
        return context.Set<EdgeGateway>().AsNoTracking()
            .FirstOrDefaultAsync(gateway => gateway.Id == gatewayId, cancellationToken);
    }

    public async Task<IReadOnlyList<EdgeGateway>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return await context.Set<EdgeGateway>().AsNoTracking()
            .Where(gateway => gateway.RanchId == ranchId)
            .OrderBy(gateway => gateway.Name).ThenBy(gateway => gateway.Id)
            .ToListAsync(cancellationToken);
    }
}
