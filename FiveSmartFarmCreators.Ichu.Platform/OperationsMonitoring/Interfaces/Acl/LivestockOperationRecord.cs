using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     A field event or clinical intervention applied to an animal.
/// </summary>
public record LivestockOperationRecord(
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
