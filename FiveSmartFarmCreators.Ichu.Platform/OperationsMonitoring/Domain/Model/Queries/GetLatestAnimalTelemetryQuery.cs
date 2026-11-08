namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Retrieves the latest valid value of each indicator of an animal and its last known position.
/// </summary>
public record GetLatestAnimalTelemetryQuery(Guid AnimalId);
