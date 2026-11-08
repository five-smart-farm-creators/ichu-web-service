using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;

public interface IPlannedActivityRepository
{
    Task AddAsync(PlannedActivity activity, CancellationToken cancellationToken);

    Task<PlannedActivity?> FindByIdAsync(PlannedActivityId activityId, CancellationToken cancellationToken);

    Task<PlannedActivity?> FindByIdAsNoTrackingAsync(PlannedActivityId activityId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PlannedActivity>> FindByRanchAndPeriodAsNoTrackingAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}
