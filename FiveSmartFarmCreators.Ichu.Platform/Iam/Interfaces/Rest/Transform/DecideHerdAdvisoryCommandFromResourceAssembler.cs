using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class DecideHerdAdvisoryCommandFromResourceAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static DecideHerdAdvisoryCommand ToCommandFromResource(Guid advisoryId, DecideAdvisoryResource resource) =>
        new(advisoryId, resource.Decision, AdvisoryScopeFromResourceAssembler.ToScope(resource.Scope), resource.ExpiresAt);
}
