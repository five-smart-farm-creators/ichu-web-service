using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="LivestockOperation"/> aggregates.
/// </summary>
public static class LivestockOperationResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="LivestockOperation"/> into a <see cref="LivestockOperationResource"/>.
    /// </summary>
    /// <param name="entity">The livestock operation aggregate root.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static LivestockOperationResource ToResourceFromEntity(LivestockOperation entity) =>
        new(
            entity.Id,
            entity.RanchId,
            entity.CattleId,
            entity.Type.ToString(),
            LivestockOperation.IsClinical(entity.Type),
            entity.OccurredAt,
            entity.ReceivedAt,
            entity.AuthorId,
            entity.Observation,
            entity.Product,
            entity.Dose);
}
