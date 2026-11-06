using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries a ranch owner's direct advisory grant details.
/// </summary>
/// <param name="RanchId">
///     The ranch whose information will be shared.
/// </param>
/// <param name="VeterinarianId">
///     The veterinarian account receiving access.
/// </param>
/// <param name="Scope">
///     The operations authorized by the grant.
/// </param>
/// <param name="ExpiresAt">
///     The time when the grant ceases to be effective.
/// </param>
public sealed record GrantHerdAdvisoryCommand(Guid RanchId, Guid VeterinarianId, AccessScope Scope, DateTimeOffset ExpiresAt);
