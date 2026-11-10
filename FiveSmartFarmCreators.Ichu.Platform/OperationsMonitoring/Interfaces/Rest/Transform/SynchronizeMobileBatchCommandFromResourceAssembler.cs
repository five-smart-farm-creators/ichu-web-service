using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class SynchronizeMobileBatchCommandFromResourceAssembler
{
    public static SynchronizeMobileBatchCommand ToCommandFromResource(MobileSyncBatchResource resource) =>
        new((resource.Operations ?? [])
            // Mobile batches may span several ranches, so their entries carry no ranch of their own.
            .Select(operation => RegisterLivestockOperationCommandFromResourceAssembler.ToCommandFromResource(null, operation))
            .ToList());
}
