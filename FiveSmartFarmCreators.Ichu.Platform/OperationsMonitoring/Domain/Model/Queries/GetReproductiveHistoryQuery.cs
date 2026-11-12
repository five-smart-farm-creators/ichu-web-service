namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Reads the complete reproductive history of an animal, which is not limited to a period.
/// </summary>
public record GetReproductiveHistoryQuery(Guid RanchId, Guid AnimalId);
