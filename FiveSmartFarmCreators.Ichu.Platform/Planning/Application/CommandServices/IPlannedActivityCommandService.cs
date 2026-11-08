using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;

/// <summary>
/// Coordinates activity, application and follow-up changes after checking ranch permissions and animal or intervention references.
/// </summary>
public interface IPlannedActivityCommandService
{
    Task<Result<PlannedActivity>> HandleAsync(SchedulePlanningActivityCommand command, CancellationToken cancellationToken);

    Task<Result<ApplicationRecord>> HandleAsync(RecordApplicationCommand command, CancellationToken cancellationToken);

    Task<Result<FollowUp>> HandleAsync(ScheduleFollowUpCommand command, CancellationToken cancellationToken);
}
