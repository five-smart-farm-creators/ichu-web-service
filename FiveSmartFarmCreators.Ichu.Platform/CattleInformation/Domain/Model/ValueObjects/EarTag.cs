using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

/// <summary>
///     Trimmed, case-sensitive ear tag. Uniqueness is scoped to a ranch, so two ranches may use the same tag.
/// </summary>
public sealed record EarTag
{
    public const int MaximumLength = 30;

    /// <summary>
    ///     Rebuilds an ear tag that was already validated when it was stored. Requests must use <see cref="Create"/>.
    /// </summary>
    public EarTag(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException("An ear tag must contain between 1 and 30 characters.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    /// <summary>
    ///     Validates an ear tag from a request, so that an invalid value becomes a business error.
    /// </summary>
    public static Result<EarTag> Create(string value) =>
        IsValid(value)
            ? Result<EarTag>.Success(new EarTag(value))
            : Result<EarTag>.Failure(CattleInformationErrors.InvalidEarTag);

    public override string ToString() => Value;

    private static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= MaximumLength;
}
