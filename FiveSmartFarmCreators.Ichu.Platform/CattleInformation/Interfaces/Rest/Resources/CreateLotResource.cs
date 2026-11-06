using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Defines a management group's name and optional criterion; the owning ranch comes from the route.
/// </summary>
public sealed record CreateLotResource(
    [Required, StringLength(80)] string Name,
    [StringLength(120)] string? Criterion);
