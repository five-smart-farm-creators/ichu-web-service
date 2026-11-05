namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>A reading attributed to an animal. Null metrics were invalid or not captured; they are never zero substitutes.</summary>
public sealed record MonitoringReadingRecord(
    Guid ReadingId,
    Guid DeviceId,
    Guid CattleId,
    DateTimeOffset RecordedAt,
    double? BodyTemperatureCelsius,
    double? ActivityIndex,
    double? RuminationMinutes,
    double? Latitude,
    double? Longitude,
    double? AccuracyMeters);
