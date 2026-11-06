namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to retrieve the latest valid value of each indicator of an animal and its last known position.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
public record GetLatestAnimalTelemetryQuery(Guid AnimalId);
