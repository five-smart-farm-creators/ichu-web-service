namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;

/// <summary>
///     Technical limits bound from the "OperationsMonitoring" configuration section.
/// </summary>
public class OperationsMonitoringOptions
{
    public const string SectionName = "OperationsMonitoring";

    /// <summary>
    ///     How far a capture or event time may be ahead of the server clock. Late arrivals are never rejected for their delay.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);

    public int MaximumBatchEntries { get; set; } = 500;

    public int MaximumPageSize { get; set; } = 500;
}
