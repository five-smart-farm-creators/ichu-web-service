namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

/// <summary>
/// Classifies a feed of a ration by its role in the diet.
/// </summary>
public enum RationKind
{
    /// <summary>
    /// Roughage such as pasture, hay or silage.
    /// </summary>
    Forage,

    /// <summary>
    /// Energy or protein feed such as grain or meal.
    /// </summary>
    Concentrate,

    /// <summary>
    /// A supplement added to the diet, such as a protein block.
    /// </summary>
    Supplement,

    /// <summary>
    /// A mineral mix offered to the herd.
    /// </summary>
    Mineral
}
