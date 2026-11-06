using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries the ranch owner's decision for a veterinarian advisory request.
/// </summary>
/// <param name="AdvisoryId">
///     The advisory request identifier.
/// </param>
/// <param name="Decision">
///     The decision to apply to the request.
/// </param>
/// <param name="Scope">
///     The access scope when the request is granted.
/// </param>
/// <param name="ExpiresAt">
///     The grant expiration, when the decision requires one.
/// </param>
public sealed record DecideHerdAdvisoryCommand(Guid AdvisoryId, string Decision, AccessScope Scope, DateTimeOffset? ExpiresAt);
