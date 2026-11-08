using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A half-open period: the start is included and the end is excluded.
/// </summary>
public record TimePeriod
{
    /// <summary>
    ///     Builds a period from instants that the calling context already ordered, as the cross-context read contract does. Requests must use <see cref="Create"/>, which reports an inverted period as a business error.
    /// </summary>
    public TimePeriod(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
        {
            throw new ArgumentException("The period start must precede its end.", nameof(from));
        }

        From = MonitoringTime.Normalize(from);
        To = MonitoringTime.Normalize(to);
    }

    public DateTimeOffset From { get; }

    public DateTimeOffset To { get; }

    public static Result<TimePeriod> Create(DateTimeOffset from, DateTimeOffset to) =>
        from < to
            ? Result<TimePeriod>.Success(new TimePeriod(from, to))
            : Result<TimePeriod>.Failure(OperationsMonitoringErrors.PeriodStartNotBeforeEnd);
}
