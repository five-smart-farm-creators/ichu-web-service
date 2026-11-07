using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Number of cattle head a subscription covers, which is also the number of collars it pays for.
/// </summary>
public readonly record struct HeadCount
{
    public const int MaximumValue = 10_000;

    /// <summary>
    ///     Creates a head count. Callers handling user input should use <see cref="Create"/> to get a result instead.
    /// </summary>
    public HeadCount(int value)
    {
        if (value is < 1 or > MaximumValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "A head count must be between 1 and the maximum.");
        }

        Value = value;
    }

    public int Value { get; }

    /// <summary>
    ///     Validates a head count that came from a request, so that an invalid number becomes a business error.
    /// </summary>
    public static Result<HeadCount> Create(int value) =>
        value is >= 1 and <= MaximumValue
            ? Result<HeadCount>.Success(new HeadCount(value))
            : Result<HeadCount>.Failure(SubscriptionPlansErrors.InvalidHeadCount);
}
