using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class AdvisoryScopeFromResourceAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static AccessScope ToScope(string? value) => value is null ? AccessScope.Read :
        Enum.TryParse<AccessScope>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed : throw new ArgumentException("The advisory scope is invalid.");
}
