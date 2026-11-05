using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record LivestockOperationResource(
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
    string? Dose)
{
    public static LivestockOperationResource From(LivestockOperation operation) =>
        new(
            operation.Id,
            operation.RanchId,
            operation.CattleId,
            operation.Type.ToString(),
            LivestockOperation.IsClinical(operation.Type),
            operation.OccurredAt,
            operation.ReceivedAt,
            operation.AuthorId,
            operation.Observation,
            operation.Product,
            operation.Dose);
}
