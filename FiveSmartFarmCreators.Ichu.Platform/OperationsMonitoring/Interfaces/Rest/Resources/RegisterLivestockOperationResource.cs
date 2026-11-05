using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// A field event (HealthEvent, Birth) or clinical intervention (Vaccination, Treatment, Insemination).
/// The client generates the operation identifier once and reuses it on every retry.
/// </summary>
public sealed record RegisterLivestockOperationResource(
    Guid OperationId,
    Guid? AnimalId,
    string? Type,
    DateTimeOffset? OccurredAt,
    string? Observation,
    string? Product,
    string? Dose)
{
    public RegisterLivestockOperationCommand ToCommand() =>
        new(OperationId, AnimalId, Type, OccurredAt, Observation, Product, Dose);
}
