namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>A half-open period: the start is included and the end is excluded.</summary>
public sealed record TimePeriod
{
    public DateTimeOffset From { get; }
    public DateTimeOffset To { get; }

    public TimePeriod(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
        {
            throw MonitoringException.InvalidInput("The period start must precede its end.");
        }

        From = from.ToUniversalTime();
        To = to.ToUniversalTime();
    }
}
