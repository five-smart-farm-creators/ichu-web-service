using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

/// <summary>
///     Projects animal state to the REST contract without exposing the domain entity.
/// </summary>
public static class CattleResourceFromEntityAssembler
{
    /// <summary>
    ///     Uses enum names in the response and orders retained stage periods by their effective start date.
    /// </summary>
    public static CattleResource ToResourceFromEntity(Cattle cattle)
    {
        var stagePeriods = cattle.StagePeriods
            .OrderBy(period => period.ValidFrom)
            .Select(period => new StagePeriodResource(
                period.Id, period.LifeStage.ToString(), period.ValidFrom, period.ValidTo))
            .ToList();

        return new CattleResource(
            cattle.Id.Value, cattle.RanchId, cattle.EarTag.Value, cattle.Breed.ToString(), cattle.Sex.ToString(),
            cattle.BirthDate, cattle.CurrentStage.ToString(), cattle.Status.ToString(),
            cattle.ExitReason?.ToString(), cattle.ExitDate, cattle.Version, stagePeriods);
    }
}
