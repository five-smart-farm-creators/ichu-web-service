using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     An omitted scope means read access, and any other text must name a defined access scope.
/// </summary>
public static class AdvisoryScopeFromResourceAssembler
{
    public static Result<AccessScope> ToScope(string? value) =>
        value is null
            ? Result<AccessScope>.Success(AccessScope.Read)
            : Enum.TryParse<AccessScope>(value, true, out var parsed) && Enum.IsDefined(parsed)
                ? Result<AccessScope>.Success(parsed)
                : Result<AccessScope>.Failure(IamErrors.InvalidAdvisoryScope);
}
