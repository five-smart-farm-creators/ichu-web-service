namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record CapturedPositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string? InvalidReason);
