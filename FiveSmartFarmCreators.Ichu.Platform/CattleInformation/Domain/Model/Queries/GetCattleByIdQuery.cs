namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

/// <summary>
/// Identifies an animal within a ranch for an authorized profile lookup, including inactive history.
/// </summary>
public sealed record GetCattleByIdQuery(Guid RanchId, Guid CattleId);
