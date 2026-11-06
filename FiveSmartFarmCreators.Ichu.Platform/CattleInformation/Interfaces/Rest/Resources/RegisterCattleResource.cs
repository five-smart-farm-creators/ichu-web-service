using System.ComponentModel.DataAnnotations;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record RegisterCattleResource(
    [Required, StringLength(30)] string EarTag,
    Breed Breed,
    Sex Sex,
    DateOnly BirthDate,
    LifeStage LifeStage);
