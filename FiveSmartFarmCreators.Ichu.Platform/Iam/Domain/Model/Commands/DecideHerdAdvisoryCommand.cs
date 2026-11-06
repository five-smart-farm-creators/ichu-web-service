using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>Defines the input for DecideHerdAdvisory.</summary>
public sealed record DecideHerdAdvisoryCommand(Guid AdvisoryId, string Decision, AccessScope Scope, DateTimeOffset? ExpiresAt);
