namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

/// <summary>
///     Normalizes the instants that the module stores and compares.
/// </summary>
public static class MonitoringTime
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    ///     Converts an instant to UTC with the microsecond precision of PostgreSQL timestamptz, so that a retried
    ///     record compares equal to the stored original.
    /// </summary>
    /// <param name="instant">The instant to normalize.</param>
    /// <returns>The instant in UTC, truncated to whole microseconds.</returns>
    public static DateTimeOffset Normalize(DateTimeOffset instant)
    {
        var ticks = instant.UtcTicks;
        return new DateTimeOffset(ticks - ticks % TicksPerMicrosecond, TimeSpan.Zero);
    }
}
