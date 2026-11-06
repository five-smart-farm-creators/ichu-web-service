using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;

/// <summary>
/// Preserves a known productive stage using an inclusive start and an exclusive end date.
/// </summary>
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
    /// <summary>
    /// Gets the exclusive end date, or null while the stage is open; same-day changes may yield an empty interval.
    /// </summary>
    public DateOnly? ValidTo { get; private set; }

    internal void Close(DateOnly validTo)
    {
        ValidTo = validTo;
    }
}
