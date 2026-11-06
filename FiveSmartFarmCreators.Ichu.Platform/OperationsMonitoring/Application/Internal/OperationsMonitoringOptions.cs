namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;

/// <summary>
///     Technical limits bound from the "OperationsMonitoring" configuration section.
/// </summary>
public sealed class OperationsMonitoringOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "OperationsMonitoring";

    /// <summary>
    ///     Gets or sets how far a capture or event time may be ahead of the server clock. Late arrivals are never
    ///     rejected for their delay.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Gets or sets the maximum number of entries of one synchronized batch.</summary>
    public int MaximumBatchEntries { get; set; } = 500;

    /// <summary>Gets or sets the maximum page size of the listings.</summary>
    public int MaximumPageSize { get; set; } = 500;
}
