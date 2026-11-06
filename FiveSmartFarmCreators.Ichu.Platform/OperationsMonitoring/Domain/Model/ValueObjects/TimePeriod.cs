namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a half-open period: the start is included and the end is excluded.
/// </summary>
public record TimePeriod
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TimePeriod"/> value object.
    /// </summary>
    /// <param name="from">The first instant of the period.</param>
    /// <param name="to">The instant that ends the period, excluded from it.</param>
    /// <exception cref="ArgumentException">Thrown when the start does not precede the end.</exception>
    public TimePeriod(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
        {
            throw new ArgumentException("The period start must precede its end.");
        }

        From = MonitoringTime.Normalize(from);
        To = MonitoringTime.Normalize(to);
    }

    /// <summary>Gets the first instant of the period, in UTC.</summary>
    public DateTimeOffset From { get; }

    /// <summary>Gets the instant that ends the period, in UTC and excluded from it.</summary>
    public DateTimeOffset To { get; }
}
