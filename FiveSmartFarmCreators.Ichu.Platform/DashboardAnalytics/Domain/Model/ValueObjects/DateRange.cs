namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Represents a UTC observation window with an inclusive start and exclusive end.
/// </summary>
public sealed record DateRange
{
    /// <summary>
    ///     Creates an observation window and normalizes its boundaries to UTC.
    /// </summary>
    /// <exception cref="ArgumentException">The start does not precede the end.</exception>
    public DateRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
        {
            throw new ArgumentException("The period start must precede its end.");
        }

        From = from.ToUniversalTime();
        To = to.ToUniversalTime();
    }

    /// <summary>
    ///     Gets the inclusive UTC start.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    ///     Gets the exclusive UTC end.
    /// </summary>
    public DateTimeOffset To { get; }

    /// <summary>
    ///     Checks whether a timestamp falls within the observation window.
    /// </summary>
    public bool Contains(DateTimeOffset timestamp) => timestamp >= From && timestamp < To;
}
