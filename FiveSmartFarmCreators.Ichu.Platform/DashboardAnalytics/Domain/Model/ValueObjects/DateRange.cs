using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     The window is half-open, so consecutive periods never count the same instant twice.
/// </summary>
public sealed record DateRange
{
    private DateRange(DateTimeOffset from, DateTimeOffset to)
    {
        From = from.ToUniversalTime();
        To = to.ToUniversalTime();
    }

    public DateTimeOffset From { get; }

    public DateTimeOffset To { get; }

    public static Result<DateRange> Create(DateTimeOffset from, DateTimeOffset to) =>
        from < to
            ? Result<DateRange>.Success(new DateRange(from, to))
            : Result<DateRange>.Failure(DashboardAnalyticsErrors.InvalidPeriod);

    public bool Contains(DateTimeOffset timestamp) => timestamp >= From && timestamp < To;
}
