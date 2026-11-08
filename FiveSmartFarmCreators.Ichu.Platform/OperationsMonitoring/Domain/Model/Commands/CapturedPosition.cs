namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A position as transmitted by an Edge Gateway, validated when its reading is processed.
/// </summary>
public record CapturedPosition(double? Latitude, double? Longitude, double? AccuracyMeters, string? InvalidReason);
