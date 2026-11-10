namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     A reading that an edge gateway holds until it synchronizes, with the metrics that a scenario sets on it.
/// </summary>
public sealed class HeldReading
{
    public required Guid ReadingId { get; init; }

    public required Guid DeviceId { get; init; }

    public required DateTimeOffset CapturedAt { get; init; }

    public object? BodyTemperature { get; set; }

    public object? RuminationMinutes { get; set; }

    public object? Position { get; set; }

    /// <summary>
    ///     Keeps the identifier and capture time, which is how an edge gateway resends or alters the same record.
    /// </summary>
    public HeldReading CopyWith(object? bodyTemperature) => new()
    {
        ReadingId = ReadingId,
        DeviceId = DeviceId,
        CapturedAt = CapturedAt,
        BodyTemperature = bodyTemperature,
        RuminationMinutes = RuminationMinutes,
        Position = Position
    };

    public object ToEntry() => new
    {
        readingId = ReadingId,
        deviceId = DeviceId,
        recordedAt = CapturedAt,
        bodyTemperature = BodyTemperature,
        ruminationMinutes = RuminationMinutes,
        position = Position
    };
}
