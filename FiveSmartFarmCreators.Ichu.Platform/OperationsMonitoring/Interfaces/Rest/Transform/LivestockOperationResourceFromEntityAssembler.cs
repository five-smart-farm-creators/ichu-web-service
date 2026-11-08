using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class LivestockOperationResourceFromEntityAssembler
{
    public static LivestockOperationResource ToResourceFromEntity(LivestockOperation entity) =>
        new(
            entity.Id.Value,
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
