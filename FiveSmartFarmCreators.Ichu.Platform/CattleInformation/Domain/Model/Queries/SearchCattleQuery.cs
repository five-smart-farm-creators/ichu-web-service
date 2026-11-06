using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

public sealed record SearchCattleQuery(Guid RanchId, string? EarTag = null, CattleStatus? Status = null, Guid? LotId = null, int Page = 1, int PageSize = 50);
