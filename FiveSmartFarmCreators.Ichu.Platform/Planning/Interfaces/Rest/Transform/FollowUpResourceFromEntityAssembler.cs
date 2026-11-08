using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects a follow-up to the REST contract with its activity identifier as a plain value.
/// </summary>
public static class FollowUpResourceFromEntityAssembler
{
    public static FollowUpResource ToResourceFromEntity(FollowUp followUp) =>
        new(followUp.Id.Value, followUp.ActivityId.Value, followUp.DueAt);
}
