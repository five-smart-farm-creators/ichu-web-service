namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
/// Classifies exits so consumers count deaths separately from sale, theft and culling.
/// </summary>
public enum ExitReason
{
    Death = 1,
    Sale = 2,
    Theft = 3,
    Culling = 4
}
