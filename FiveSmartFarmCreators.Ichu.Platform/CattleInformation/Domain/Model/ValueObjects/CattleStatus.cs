namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
/// Separates active inventory from animal records retained after an exit.
/// </summary>
public enum CattleStatus
{
    Active = 1,
    Inactive = 2
}
