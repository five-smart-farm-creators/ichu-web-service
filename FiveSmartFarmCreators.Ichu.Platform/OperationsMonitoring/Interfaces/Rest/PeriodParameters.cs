using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

public static class PeriodParameters
{
    public static TimePeriod Require(DateTimeOffset? from, DateTimeOffset? to) =>
        from is { } start && to is { } end
            ? new TimePeriod(start, end)
            : throw MonitoringException.InvalidInput("Both from and to ISO 8601 timestamps are required.");
}
