using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record CreateLotResource(
    [Required, StringLength(80)] string Name,
    [StringLength(120)] string? Criterion);
