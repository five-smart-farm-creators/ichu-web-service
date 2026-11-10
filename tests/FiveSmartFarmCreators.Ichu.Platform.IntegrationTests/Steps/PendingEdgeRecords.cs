namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Holds the entries that an edge gateway has not synchronized yet, so that a scenario can compose a batch step by step.
/// </summary>
public sealed class PendingEdgeRecords
{
    private readonly List<HeldReading> _readings = [];
    private readonly List<object> _alerts = [];
    private readonly List<object> _alertChanges = [];

    /// <summary>
    ///     Every reading held in the scenario by alias, so that later steps can modify or copy it.
    /// </summary>
    public Dictionary<string, HeldReading> HeldReadings { get; } = new(StringComparer.Ordinal);

    public void HoldReading(string alias, HeldReading reading)
    {
        HeldReadings[alias] = reading;
        _readings.Add(reading);
    }

    public void HoldAlert(object alert) => _alerts.Add(alert);

    public void HoldAlertChange(object change) => _alertChanges.Add(change);

    /// <summary>
    ///     Builds the batch of the held entries and releases them, because the gateway keeps only what it has not yet synchronized.
    /// </summary>
    public object TakeBatch()
    {
        var batch = new
        {
            readings = _readings.Select(reading => reading.ToEntry()).ToList(),
            alerts = _alerts.ToList(),
            alertChanges = _alertChanges.ToList()
        };
        _readings.Clear();
        _alerts.Clear();
        _alertChanges.Clear();
        return batch;
    }
}
