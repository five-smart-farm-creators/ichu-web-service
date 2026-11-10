using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record DecideAdvisoryResource([Required] string Decision, string? Scope, DateTimeOffset? ExpiresAt);
