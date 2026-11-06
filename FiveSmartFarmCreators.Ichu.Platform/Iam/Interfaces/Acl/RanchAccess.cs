using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

/// <summary>
///     Projects the effective scope and optional expiration of ranch access.
/// </summary>
public sealed record RanchAccess(Guid RanchId, AccessScope Scope, DateTimeOffset? ExpiresAt);
