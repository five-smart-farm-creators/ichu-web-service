using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

public sealed record EarTag
{
    public const int MaximumLength = 30;

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
