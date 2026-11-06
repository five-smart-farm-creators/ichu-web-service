namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
/// Defines a ranch management group with an optional description of its grouping criterion.
/// </summary>
public sealed record CreateLotCommand(Guid RanchId, string Name, string? Criterion);
