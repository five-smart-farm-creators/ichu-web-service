using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the owner's decision, optional scope, and expiration for an advisory request.
/// </summary>
public sealed record DecideAdvisoryResource([Required] string Decision, string? Scope, DateTimeOffset? ExpiresAt);
