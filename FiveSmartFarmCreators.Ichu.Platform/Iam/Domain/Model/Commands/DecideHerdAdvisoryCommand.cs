using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

public sealed record DecideHerdAdvisoryCommand(Guid RanchId, Guid AdvisoryId, string Decision, AccessScope Scope, DateTimeOffset? ExpiresAt);
