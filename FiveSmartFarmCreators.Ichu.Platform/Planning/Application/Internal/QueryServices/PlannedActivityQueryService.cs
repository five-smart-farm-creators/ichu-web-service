using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;

/// <summary>
/// Reads planned activities, their records and the calendar for accounts that may read their ranch.
/// </summary>
public sealed class PlannedActivityQueryService(
    IPlannedActivityRepository activities,
    PlanningGuard guard) : IPlannedActivityQueryService
{
    /// <inheritdoc />
    public Task<Result<PlannedActivity>> HandleAsync(GetPlannedActivityByIdQuery query, CancellationToken cancellationToken) =>
        FindReadableAsync(query.RanchId, query.ActivityId, cancellationToken);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PlannedActivity>>> HandleAsync(GetPlanningCalendarQuery query, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<PlannedActivity>>.Failure(access.DomainError);
        }

        if (query.From.Offset != TimeSpan.Zero || query.To.Offset != TimeSpan.Zero)
        {
            return Result<IReadOnlyList<PlannedActivity>>.Failure(PlanningErrors.UtcTimestampRequired);
        }

        if (query.To < query.From)
        {
            return Result<IReadOnlyList<PlannedActivity>>.Failure(PlanningErrors.CalendarRangeInvalid);
        }

        return Result<IReadOnlyList<PlannedActivity>>.Success(
            await activities.FindByRanchAndPeriodAsNoTrackingAsync(query.RanchId, query.From, query.To, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ApplicationRecord>>> HandleAsync(
        GetApplicationRecordsByActivityQuery query,
        CancellationToken cancellationToken)
    {
        var activity = await FindReadableAsync(query.RanchId, query.ActivityId, cancellationToken);
        if (activity.IsFailure)
        {
            return Result<IReadOnlyList<ApplicationRecord>>.Failure(activity.DomainError);
        }

        return Result<IReadOnlyList<ApplicationRecord>>.Success(
            activity.Value!.Applications.OrderBy(application => application.AppliedAt).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FollowUp>>> HandleAsync(GetFollowUpsByActivityQuery query, CancellationToken cancellationToken)
    {
        var activity = await FindReadableAsync(query.RanchId, query.ActivityId, cancellationToken);
        if (activity.IsFailure)
        {
            return Result<IReadOnlyList<FollowUp>>.Failure(activity.DomainError);
        }

        return Result<IReadOnlyList<FollowUp>>.Success(
            activity.Value!.FollowUps.OrderBy(followUp => followUp.DueAt).ToList());
    }

    private async Task<Result<PlannedActivity>> FindReadableAsync(Guid ranchId, PlannedActivityId activityId, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(ranchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PlannedActivity>.Failure(access.DomainError);
        }

        // The ranch is checked before the lookup, and an activity of another ranch is reported as missing, so identifiers cannot be probed.
        var activity = await activities.FindByIdAsNoTrackingAsync(activityId, cancellationToken);
        if (activity is null || activity.RanchId != ranchId)
        {
            return Result<PlannedActivity>.Failure(PlanningErrors.ActivityNotFound);
        }

        return Result<PlannedActivity>.Success(activity);
    }
}
