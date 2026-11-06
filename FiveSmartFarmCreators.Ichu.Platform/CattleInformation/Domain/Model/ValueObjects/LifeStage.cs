namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
/// Defines accepted productive stages without inferring transitions from age or sex.
/// </summary>
public enum LifeStage
{
    Calf = 1,
    Heifer = 2,
    MilkingCow = 3,
    FatteningBull = 4
}
