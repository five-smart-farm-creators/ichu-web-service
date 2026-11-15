using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
///     Allows identity and productive-stage changes. Omitted or null editable fields retain their current values.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PatchCattleResource(
    [Required] Guid? ExpectedVersion,
    [StringLength(30, MinimumLength = 1)] string? EarTag = null,
    Breed? Breed = null,
    Sex? Sex = null,
    DateOnly? BirthDate = null,
    LifeStage? LifeStage = null,
    DateOnly? ValidFrom = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EarTag is null && Breed is null && Sex is null && BirthDate is null && LifeStage is null)
        {
            yield return new ValidationResult("At least one editable animal field is required.",
                [nameof(EarTag), nameof(Breed), nameof(Sex), nameof(BirthDate), nameof(LifeStage)]);
        }

        if (LifeStage.HasValue != ValidFrom.HasValue)
        {
            yield return new ValidationResult("Life stage and validFrom must be supplied together.",
                [nameof(LifeStage), nameof(ValidFrom)]);
        }
    }
}
