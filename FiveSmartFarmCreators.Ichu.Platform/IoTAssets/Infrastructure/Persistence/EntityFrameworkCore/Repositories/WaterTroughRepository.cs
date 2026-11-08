using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class WaterTroughRepository(AppDbContext context) : IWaterTroughRepository
{
    public async Task AddAsync(WaterTrough waterTrough, CancellationToken cancellationToken)
    {
        await context.Set<WaterTrough>().AddAsync(waterTrough, cancellationToken);
    }

    public Task<WaterTrough?> FindByIdAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken)
    {
        return context.Set<WaterTrough>()
            .FirstOrDefaultAsync(t => t.RanchId == ranchId && t.Id == waterTroughId, cancellationToken);
    }

    public Task<WaterTrough?> FindByIdAsNoTrackingAsync(Guid ranchId, WaterTroughId waterTroughId, CancellationToken cancellationToken)
    {
        return context.Set<WaterTrough>().AsNoTracking()
            .FirstOrDefaultAsync(t => t.RanchId == ranchId && t.Id == waterTroughId, cancellationToken);
    }

    public async Task<IReadOnlyList<WaterTrough>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return await context.Set<WaterTrough>().AsNoTracking().Where(t => t.RanchId == ranchId)
            .OrderBy(t => t.Name).ThenBy(t => t.Id).ToListAsync(cancellationToken);
    }
}
