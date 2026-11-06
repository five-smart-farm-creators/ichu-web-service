using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;

public sealed class StagePeriod
{
    private StagePeriod()
    {
    }

    internal StagePeriod(Guid cattleId, LifeStage lifeStage, DateOnly validFrom)
    {
        Id = Guid.NewGuid();
        CattleId = cattleId;
        LifeStage = lifeStage;
        ValidFrom = validFrom;
    }

    public Guid Id { get; private set; }
    public Guid CattleId { get; private set; }
    public LifeStage LifeStage { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }

    internal void Close(DateOnly validTo)
    {
        ValidTo = validTo;
    }
}
