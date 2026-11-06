using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
/// Represents a trimmed, case-sensitive ear tag whose uniqueness is scoped to a ranch.
/// </summary>
public sealed record EarTag
{
    public const int MaximumLength = 30;

    /// <summary>
    /// Validates an ear tag without imposing a numeric format or changing its case.
    /// </summary>
    /// <exception cref="CattleInformationException">The trimmed value is empty or exceeds the maximum length.</exception>
    public EarTag(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaximumLength)
        {
            throw new CattleInformationException(
                "invalid_input", "The ear tag must contain between 1 and 30 characters.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
