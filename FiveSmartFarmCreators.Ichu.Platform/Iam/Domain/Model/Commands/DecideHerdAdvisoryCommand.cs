using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <param name="RanchId">The ranch from a scoped route, or null to resolve it from the stored request.</param>
public sealed record DecideHerdAdvisoryCommand(Guid? RanchId, Guid AdvisoryId, string Decision, AccessScope Scope, DateTimeOffset? ExpiresAt);
