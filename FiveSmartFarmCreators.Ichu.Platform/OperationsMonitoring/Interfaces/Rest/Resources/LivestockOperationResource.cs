namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record LivestockOperationResource(
    Guid OperationId,
    Guid RanchId,
    Guid AnimalId,
    string Type,
    bool IsClinical,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReceivedAt,
    Guid AuthorId,
    string? Observation,
    string? Product,
    string? Dose);
