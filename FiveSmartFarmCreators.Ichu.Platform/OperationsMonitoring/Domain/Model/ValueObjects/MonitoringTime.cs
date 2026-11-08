namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Normalizes the instants that the module stores and compares.
/// </summary>
public static class MonitoringTime
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    ///     Truncates an instant to the microsecond precision of PostgreSQL timestamptz, so that a retried record compares equal to the stored original.
    /// </summary>
    public static DateTimeOffset Normalize(DateTimeOffset instant)
    {
        var ticks = instant.UtcTicks;
        return new DateTimeOffset(ticks - ticks % TicksPerMicrosecond, TimeSpan.Zero);
    }
}
