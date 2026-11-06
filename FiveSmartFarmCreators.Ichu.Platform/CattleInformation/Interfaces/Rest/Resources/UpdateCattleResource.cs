using System.ComponentModel.DataAnnotations;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Proposes active animal identity corrections guarded by its version; stage changes use a separate resource.
/// </summary>
public sealed record UpdateCattleResource(
    [Required, StringLength(30)] string EarTag,
    Breed Breed,
    Sex Sex,
    DateOnly BirthDate,
    Guid ExpectedVersion);
