using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class DecideHerdAdvisoryCommandFromResourceAssembler
{
    public static Result<DecideHerdAdvisoryCommand> ToCommandFromResource(Guid? ranchId, Guid advisoryId, DecideAdvisoryResource resource)
    {
        var scope = AdvisoryScopeFromResourceAssembler.ToScope(resource.Scope);
        return scope.IsFailure
            ? Result<DecideHerdAdvisoryCommand>.Failure(scope.DomainError)
            : Result<DecideHerdAdvisoryCommand>.Success(
                new DecideHerdAdvisoryCommand(ranchId, advisoryId, resource.Decision, scope.Value!, resource.ExpiresAt));
    }
}
