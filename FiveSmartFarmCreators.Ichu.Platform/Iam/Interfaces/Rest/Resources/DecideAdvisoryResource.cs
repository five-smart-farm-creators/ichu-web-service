using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the DecideAdvisoryResource REST representation.</summary>
public sealed record DecideAdvisoryResource([Required] string Decision, string? Scope, DateTimeOffset? ExpiresAt);
