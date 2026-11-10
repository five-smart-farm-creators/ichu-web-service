using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class GrantHerdAdvisoryCommandFromResourceAssembler
{
    public static Result<GrantHerdAdvisoryCommand> ToCommandFromResource(Guid ranchId, GrantAdvisoryResource resource)
    {
        var scope = AdvisoryScopeFromResourceAssembler.ToScope(resource.Scope);
        return scope.IsFailure
            ? Result<GrantHerdAdvisoryCommand>.Failure(scope.DomainError)
            : Result<GrantHerdAdvisoryCommand>.Success(
                new GrantHerdAdvisoryCommand(ranchId, resource.VeterinarianId, scope.Value!, resource.ExpiresAt));
    }
}
