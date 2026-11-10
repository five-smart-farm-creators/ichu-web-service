using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

public sealed record CheckRanchAccessQuery(Guid AccountId, Guid RanchId, AccessScope Scope);
