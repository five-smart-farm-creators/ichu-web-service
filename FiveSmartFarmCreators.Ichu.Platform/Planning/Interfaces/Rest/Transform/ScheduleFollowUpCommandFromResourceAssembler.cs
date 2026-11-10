using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the follow-up request body and its route ranch and activity to the command, typing the activity identifier.
/// </summary>
public static class ScheduleFollowUpCommandFromResourceAssembler
{
    public static ScheduleFollowUpCommand ToCommandFromResource(Guid ranchId, Guid activityId, ScheduleFollowUpResource resource) =>
        new(ranchId, new PlannedActivityId(activityId), resource.DueAt);
}
