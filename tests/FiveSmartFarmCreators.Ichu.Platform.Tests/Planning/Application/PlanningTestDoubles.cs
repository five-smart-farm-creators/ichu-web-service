using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

internal sealed class FakePlanningAccess : IPlanningAccessControl
{
    public Guid? AccountId { get; set; }

    public IReadOnlyCollection<PlanningPermission> Grants { get; set; } = Enum.GetValues<PlanningPermission>();

    public HashSet<Guid> DeniedRanches { get; } = [];

    public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult(AccountId);

    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, PlanningPermission permission, CancellationToken cancellationToken) =>
        Task.FromResult(Grants.Contains(permission) && !DeniedRanches.Contains(ranchId));
}

internal sealed class FakeCattleLookup : IPlanningCattleLookup
{
    public HashSet<(Guid RanchId, Guid CattleId)> Members { get; } = [];

    public Task<bool> CattleBelongsToRanchAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
        Task.FromResult(Members.Contains((ranchId, cattleId)));
}

internal sealed class FakeInterventionLookup : IClinicalInterventionLookup
{
    public Dictionary<Guid, ClinicalInterventionSnapshot> Interventions { get; } = [];

    public Task<ClinicalInterventionSnapshot?> FindAsync(Guid interventionId, CancellationToken cancellationToken) =>
        Task.FromResult(Interventions.GetValueOrDefault(interventionId));
}

internal sealed class FakeCampaigns : IPlanningCampaignRepository
{
    public List<PlanningCampaign> Items { get; } = [];

    public Task AddAsync(PlanningCampaign campaign, CancellationToken cancellationToken)
    {
        Items.Add(campaign);
        return Task.CompletedTask;
    }

    public Task<PlanningCampaign?> FindByIdAsNoTrackingAsync(PlanningCampaignId campaignId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(campaign => campaign.Id == campaignId));

    public Task<IReadOnlyList<PlanningCampaign>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PlanningCampaign>>(Items.Where(campaign => campaign.RanchId == ranchId).ToList());
}

internal sealed class FakeActivities : IPlannedActivityRepository
{
    public List<PlannedActivity> Items { get; } = [];

    public Task AddAsync(PlannedActivity activity, CancellationToken cancellationToken)
    {
        Items.Add(activity);
        return Task.CompletedTask;
    }

    public Task<PlannedActivity?> FindByIdAsync(PlannedActivityId activityId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(activity => activity.Id == activityId));

    public Task<PlannedActivity?> FindByIdAsNoTrackingAsync(PlannedActivityId activityId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(activity => activity.Id == activityId));

    public Task<IReadOnlyList<PlannedActivity>> FindByRanchAndPeriodAsNoTrackingAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PlannedActivity>>(Items
            .Where(activity => activity.RanchId == ranchId && activity.ScheduledAt >= from && activity.ScheduledAt <= to)
            .ToList());
}

internal sealed class FakeWithdrawalPeriods : IWithdrawalPeriodRepository
{
    public List<WithdrawalPeriod> Items { get; } = [];

    public Task AddAsync(WithdrawalPeriod withdrawalPeriod, CancellationToken cancellationToken)
    {
        Items.Add(withdrawalPeriod);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<WithdrawalPeriod>> FindByRanchAsNoTrackingAsync(Guid ranchId, Guid? cattleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WithdrawalPeriod>>(Items.Where(period => period.RanchId == ranchId).ToList());
}

internal sealed class CountingUnitOfWork : IUnitOfWork
{
    public int CompletedCount { get; private set; }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        CompletedCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeLotLookup : IPlanningLotLookup
{
    public HashSet<(Guid RanchId, Guid LotId)> Lots { get; } = [];

    public Task<Guid?> FindRanchIdAsync(Guid lotId, CancellationToken cancellationToken) =>
        Task.FromResult(Lots
            .Where(lot => lot.LotId == lotId)
            .Select(lot => (Guid?)lot.RanchId)
            .FirstOrDefault());
}

internal sealed class FakeNutritionPlans : INutritionPlanRepository
{
    public List<NutritionPlan> Items { get; } = [];

    public Task AddAsync(NutritionPlan plan, CancellationToken cancellationToken)
    {
        Items.Add(plan);
        return Task.CompletedTask;
    }

    // Mirrors the database filter: the overlapping plans of the lot and its open-ended plan.
    public Task<IReadOnlyList<NutritionPlan>> FindOverlappingByLotAsync(Guid lotId, DateOnly validFrom, DateOnly? validTo, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<NutritionPlan>>(Items
            .Where(plan => plan.LotId == lotId
                && (plan.ValidTo is null || plan.ValidTo >= validFrom)
                && (validTo is null || plan.ValidFrom <= validTo))
            .ToList());

    public Task<IReadOnlyList<NutritionPlan>> FindByLotAsNoTrackingAsync(Guid lotId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<NutritionPlan>>(Items
            .Where(plan => plan.LotId == lotId)
            .OrderByDescending(plan => plan.ValidFrom)
            .ToList());

    public Task<NutritionPlan?> FindCurrentByLotAsNoTrackingAsync(Guid lotId, DateOnly on, CancellationToken cancellationToken) =>
        Task.FromResult(Items
            .Where(plan => plan.LotId == lotId && plan.ValidFrom <= on && (plan.ValidTo is null || plan.ValidTo >= on))
            .OrderByDescending(plan => plan.ValidFrom)
            .FirstOrDefault());
}

internal sealed class ImmediateTransaction : IPlanningTransaction
{
    public Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, CancellationToken cancellationToken) =>
        operation(cancellationToken);
}
