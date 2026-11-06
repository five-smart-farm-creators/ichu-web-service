using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class GrantHerdAdvisoryCommandFromResourceAssembler
{
    public static GrantHerdAdvisoryCommand ToCommandFromResource(GrantAdvisoryResource resource) =>
        new(resource.RanchId, resource.VeterinarianId, AdvisoryScopeFromResourceAssembler.ToScope(resource.Scope), resource.ExpiresAt);
}
