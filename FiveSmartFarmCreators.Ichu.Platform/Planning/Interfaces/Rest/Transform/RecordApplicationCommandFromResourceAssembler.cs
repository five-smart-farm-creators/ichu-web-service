using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the application request body and its route ranch and activity to the command, typing the activity identifier.
/// </summary>
public static class RecordApplicationCommandFromResourceAssembler
{
    public static RecordApplicationCommand ToCommandFromResource(Guid ranchId, Guid activityId, RecordApplicationResource resource) =>
        new(ranchId, new PlannedActivityId(activityId), resource.CattleId, resource.AppliedAt, resource.InterventionId);
}
