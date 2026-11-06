namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A position as transmitted by an Edge Gateway, validated when its reading is processed.
/// </summary>
/// <param name="Latitude">The latitude in decimal degrees, if any.</param>
/// <param name="Longitude">The longitude in decimal degrees, if any.</param>
/// <param name="AccuracyMeters">The declared accuracy in meters, if any.</param>
/// <param name="InvalidReason">The reason the device marked the capture as invalid, if it did.</param>
public record CapturedPosition(double? Latitude, double? Longitude, double? AccuracyMeters, string? InvalidReason);
