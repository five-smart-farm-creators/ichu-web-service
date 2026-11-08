using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;

/// <summary>
/// Reads planned activities and their records after checking the caller's read permission on their ranch.
/// </summary>
public interface IPlannedActivityQueryService
{
    Task<Result<PlannedActivity>> HandleAsync(GetPlannedActivityByIdQuery query, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PlannedActivity>>> HandleAsync(GetPlanningCalendarQuery query, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ApplicationRecord>>> HandleAsync(GetApplicationRecordsByActivityQuery query, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<FollowUp>>> HandleAsync(GetFollowUpsByActivityQuery query, CancellationToken cancellationToken);
}
