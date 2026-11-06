using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

/// <summary>
/// Filters one ranch by an exact trimmed tag, status or lot; a null status includes inactive cattle.
/// Page numbers start at one and application services accept page sizes from one through 100.
/// </summary>
public sealed record SearchCattleQuery(Guid RanchId, string? EarTag = null, CattleStatus? Status = null, Guid? LotId = null, int Page = 1, int PageSize = 50);
