using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Converts the optional REST scope into a domain access scope.
/// </summary>
public static class AdvisoryScopeFromResourceAssembler
{
    /// <summary>
    ///     Defaults an omitted scope to read access and rejects invalid values.
    /// </summary>
    /// <param name="value">
    ///     The optional scope supplied by the client.
    /// </param>
    /// <returns>
    ///     The matching access scope, or read access when omitted.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     The supplied scope is not a defined access scope.
    /// </exception>
    public static AccessScope ToScope(string? value) => value is null ? AccessScope.Read :
        Enum.TryParse<AccessScope>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed : throw new ArgumentException("The advisory scope is invalid.");
}
