using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores planned activities with their application records and follow-ups, which are always loaded with their activity.
/// </summary>
public sealed class PlannedActivityRepository(AppDbContext context) : IPlannedActivityRepository
{
    /// <inheritdoc />
    public async Task AddAsync(PlannedActivity activity, CancellationToken cancellationToken)
    {
        await context.Set<PlannedActivity>().AddAsync(activity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlannedActivity?> FindByIdAsync(PlannedActivityId activityId, CancellationToken cancellationToken) =>
        WithRecords().FirstOrDefaultAsync(activity => activity.Id == activityId, cancellationToken);

    /// <inheritdoc />
    public Task<PlannedActivity?> FindByIdAsNoTrackingAsync(PlannedActivityId activityId, CancellationToken cancellationToken) =>
        WithRecords().AsNoTracking().FirstOrDefaultAsync(activity => activity.Id == activityId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlannedActivity>> FindByRanchAndPeriodAsNoTrackingAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await context.Set<PlannedActivity>()
            .AsNoTracking()
            .Where(activity => activity.RanchId == ranchId && activity.ScheduledAt >= from && activity.ScheduledAt <= to)
            .OrderBy(activity => activity.ScheduledAt)
            .ThenBy(activity => activity.Id)
            .ToListAsync(cancellationToken);

    private IQueryable<PlannedActivity> WithRecords() =>
        context.Set<PlannedActivity>()
            .Include(activity => activity.Applications)
            .Include(activity => activity.FollowUps)
            .AsSplitQuery();
}
