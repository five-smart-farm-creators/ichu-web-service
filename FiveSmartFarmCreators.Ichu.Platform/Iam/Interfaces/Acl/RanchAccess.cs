using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

public sealed record RanchAccess(Guid RanchId, AccessScope Scope, DateTimeOffset? ExpiresAt);
