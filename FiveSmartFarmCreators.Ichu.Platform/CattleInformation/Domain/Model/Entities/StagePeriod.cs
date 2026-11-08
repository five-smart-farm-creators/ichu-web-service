using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;

/// <summary>
///     Preserves a productive stage with an inclusive start date and an exclusive end date.
/// </summary>
public sealed class StagePeriod
{
    // Entity Framework Core materializes periods without running the constructor.
    private StagePeriod()
    {
    }

    internal StagePeriod(CattleId cattleId, LifeStage lifeStage, DateOnly validFrom)
    {
        Id = Guid.NewGuid();
        CattleId = cattleId;
        LifeStage = lifeStage;
        ValidFrom = validFrom;
    }

    public Guid Id { get; private set; }

    public CattleId CattleId { get; private set; }

    public LifeStage LifeStage { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    ///     Gets the exclusive end date; null keeps the stage open. A same-day change may leave an empty interval.
    /// </summary>
    public DateOnly? ValidTo { get; private set; }

    internal void Close(DateOnly validTo)
    {
        ValidTo = validTo;
    }
}
