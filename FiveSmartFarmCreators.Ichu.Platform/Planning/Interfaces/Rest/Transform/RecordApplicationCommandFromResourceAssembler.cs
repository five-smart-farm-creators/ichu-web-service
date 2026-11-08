using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the application request body and its route activity to the command, typing the activity identifier.
/// </summary>
public static class RecordApplicationCommandFromResourceAssembler
{
    public static RecordApplicationCommand ToCommandFromResource(Guid activityId, RecordApplicationResource resource) =>
        new(new PlannedActivityId(activityId), resource.CattleId, resource.AppliedAt, resource.InterventionId);
}
