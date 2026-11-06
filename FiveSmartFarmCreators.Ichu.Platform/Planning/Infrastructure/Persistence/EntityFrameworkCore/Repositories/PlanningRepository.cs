using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Persists Planning records using EF Core.
/// </summary>
public sealed class PlanningRepository : IPlanningRepository
{
    private readonly PlanningDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The Planning EF Core context.</param>
    public PlanningRepository(PlanningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddCampaignAsync(PlanningCampaign campaign, CancellationToken cancellationToken)
    {
        await _dbContext.Campaigns.AddAsync(campaign, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlanningCampaign?> FindCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        return _dbContext.Campaigns
            .AsNoTracking()
            .SingleOrDefaultAsync(campaign => campaign.Id == campaignId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlanningCampaign>> FindCampaignsAsync(
        Guid ranchId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Campaigns
            .AsNoTracking()
            .Where(campaign => campaign.RanchId == ranchId)
            .OrderBy(campaign => campaign.StartsOn)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddActivityAsync(PlannedActivity activity, CancellationToken cancellationToken)
    {
        await _dbContext.Activities.AddAsync(activity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlannedActivity?> FindActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        return _dbContext.Activities
            .AsNoTracking()
            .SingleOrDefaultAsync(activity => activity.Id == activityId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlannedActivity>> FindActivitiesAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.RanchId == ranchId &&
                activity.ScheduledAt >= from &&
                activity.ScheduledAt <= to)
            .OrderBy(activity => activity.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddApplicationAsync(ApplicationRecord application, CancellationToken cancellationToken)
    {
        await _dbContext.Applications.AddAsync(application, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationRecord>> FindApplicationsAsync(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Applications
            .AsNoTracking()
            .Where(application => application.ActivityId == activityId)
            .OrderBy(application => application.AppliedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddFollowUpAsync(FollowUp followUp, CancellationToken cancellationToken)
    {
        await _dbContext.FollowUps.AddAsync(followUp, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FollowUp>> FindFollowUpsAsync(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FollowUps
            .AsNoTracking()
            .Where(followUp => followUp.ActivityId == activityId)
            .OrderBy(followUp => followUp.DueAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddWithdrawalPeriodAsync(WithdrawalPeriod withdrawalPeriod, CancellationToken cancellationToken)
    {
        await _dbContext.WithdrawalPeriods.AddAsync(withdrawalPeriod, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WithdrawalPeriod>> FindWithdrawalPeriodsAsync(
        Guid ranchId,
        Guid? cattleId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.WithdrawalPeriods
            .AsNoTracking()
            .Where(period => period.RanchId == ranchId);

        if (cattleId is Guid requestedCattleId)
        {
            query = query.Where(period => period.CattleId == requestedCattleId);
        }

        return await query
            .OrderBy(period => period.StartsAt)
            .ToListAsync(cancellationToken);
    }
}
