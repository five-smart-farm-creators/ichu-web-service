namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record PositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string Status, string? InvalidReason);
