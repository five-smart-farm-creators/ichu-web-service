using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

public static class CattleResourceFromEntityAssembler
{
    public static CattleResource ToResourceFromEntity(Cattle cattle)
    {
        var stagePeriods = cattle.StagePeriods
            .OrderBy(period => period.ValidFrom)
            .Select(period => new StagePeriodResource(
                period.Id, period.LifeStage.ToString(), period.ValidFrom, period.ValidTo))
            .ToList();

        return new CattleResource(
            cattle.Id, cattle.RanchId, cattle.EarTag.Value, cattle.Breed.ToString(), cattle.Sex.ToString(),
            cattle.BirthDate, cattle.CurrentStage.ToString(), cattle.Status.ToString(),
            cattle.ExitReason?.ToString(), cattle.ExitDate, cattle.Version, stagePeriods);
    }
}
