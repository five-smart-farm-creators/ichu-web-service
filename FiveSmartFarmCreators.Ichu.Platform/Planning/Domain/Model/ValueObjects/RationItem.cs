using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

/// <summary>
/// One feed of a ration with the quantity that each head receives every day. Its values are validated once, when it is created.
/// </summary>
public sealed record RationItem
{
    public const int MaximumFeedLength = 80;
    public const decimal MaximumKilogramsPerHeadPerDay = 100m;

    private RationItem(string feed, RationKind kind, decimal kilogramsPerHeadPerDay)
    {
        Feed = feed;
        Kind = kind;
        KilogramsPerHeadPerDay = kilogramsPerHeadPerDay;
    }

    public string Feed { get; }

    public RationKind Kind { get; }

    public decimal KilogramsPerHeadPerDay { get; }

    public static Result<RationItem> Create(string? feed, RationKind? kind, decimal? kilogramsPerHeadPerDay)
    {
        var normalizedFeed = feed?.Trim();
        if (string.IsNullOrEmpty(normalizedFeed))
        {
            return Result<RationItem>.Failure(PlanningErrors.RationFeedRequired);
        }

        if (normalizedFeed.Length > MaximumFeedLength)
        {
            return Result<RationItem>.Failure(PlanningErrors.RationFeedTooLong);
        }

        if (kind is not { } definedKind || !Enum.IsDefined(definedKind))
        {
            return Result<RationItem>.Failure(PlanningErrors.RationKindInvalid);
        }

        if (kilogramsPerHeadPerDay is not { } quantity || quantity <= 0 || quantity > MaximumKilogramsPerHeadPerDay)
        {
            return Result<RationItem>.Failure(PlanningErrors.RationQuantityInvalid);
        }

        return Result<RationItem>.Success(new RationItem(normalizedFeed, definedKind, quantity));
    }
}
