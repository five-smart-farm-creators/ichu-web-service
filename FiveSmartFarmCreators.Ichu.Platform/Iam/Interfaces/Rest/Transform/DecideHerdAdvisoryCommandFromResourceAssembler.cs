using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class DecideHerdAdvisoryCommandFromResourceAssembler
{
    public static DecideHerdAdvisoryCommand ToCommandFromResource(Guid advisoryId, DecideAdvisoryResource resource) =>
        new(advisoryId, resource.Decision, AdvisoryScopeFromResourceAssembler.ToScope(resource.Scope), resource.ExpiresAt);
}
