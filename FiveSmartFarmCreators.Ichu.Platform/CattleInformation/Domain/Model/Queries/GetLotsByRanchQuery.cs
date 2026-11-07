namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

/// <summary>
/// Requests stored ranch lots and memberships, including memberships retained after an animal exits.
/// </summary>
public sealed record GetLotsByRanchQuery(Guid RanchId);
