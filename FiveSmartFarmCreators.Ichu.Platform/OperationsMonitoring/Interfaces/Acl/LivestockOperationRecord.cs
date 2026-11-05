using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

public sealed record LivestockOperationRecord(
    Guid OperationId,
    Guid RanchId,
    Guid CattleId,
    OperationType Type,
    bool IsClinical,
    DateTimeOffset OccurredAt,
    Guid AuthorId,
    string? Product,
    string? Dose,
    string? Observation);
