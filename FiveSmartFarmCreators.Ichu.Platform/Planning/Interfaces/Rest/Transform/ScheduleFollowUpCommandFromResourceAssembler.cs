using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the follow-up request body and route activity to the command, typing the activity identifier.
/// </summary>
public static class ScheduleFollowUpCommandFromResourceAssembler
{
    public static ScheduleFollowUpCommand ToCommandFromResource(Guid activityId, ScheduleFollowUpResource resource) =>
        new(new PlannedActivityId(activityId), resource.DueAt);
}
