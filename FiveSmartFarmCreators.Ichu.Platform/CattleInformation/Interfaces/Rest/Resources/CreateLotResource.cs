using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Defines a management group's name and optional grouping reason; the owning ranch comes from the route.
/// </summary>
public sealed record CreateLotResource(
    [Required, StringLength(80)] string Name,
    [property: Description("Optional plain-text reason for grouping the animals, displayed in lot lists and exports. Example: Animals in lactation. Maximum 120 characters; surrounding whitespace is trimmed, and blank or omitted text is stored as null.")]
    [StringLength(120)] string? Criterion = null);
