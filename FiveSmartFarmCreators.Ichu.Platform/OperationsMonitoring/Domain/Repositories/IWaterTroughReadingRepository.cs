using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IWaterTroughReadingRepository
{
    Task AddAsync(WaterTroughReading reading, CancellationToken cancellationToken);
    Task<WaterTroughReading?> FindByControllerAndReadingIdAsync(Guid controllerId, Guid readingId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<WaterTroughReading>> FindByControllerAsync(Guid ranchId, Guid controllerId,
        TimePeriod period, int skip, int take, CancellationToken cancellationToken);
    Task<int> CountByControllerAsync(Guid ranchId, Guid controllerId, TimePeriod period,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<WaterTroughReading>> FindLatestByRanchAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WaterTroughReading>> FindLatestReceivedByRanchAsync(Guid ranchId, CancellationToken cancellationToken);
}
