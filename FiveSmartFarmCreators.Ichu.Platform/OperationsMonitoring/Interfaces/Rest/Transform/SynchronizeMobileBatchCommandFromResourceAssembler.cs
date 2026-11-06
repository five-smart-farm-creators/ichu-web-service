using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the mobile batch synchronization command from its REST resource.
/// </summary>
public static class SynchronizeMobileBatchCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="MobileSyncBatchResource"/> into a <see cref="SynchronizeMobileBatchCommand"/>.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the recorded operations.</param>
    /// <returns>The command to synchronize the operations.</returns>
    public static SynchronizeMobileBatchCommand ToCommandFromResource(MobileSyncBatchResource resource) =>
        new((resource.Operations ?? [])
            .Select(RegisterLivestockOperationCommandFromResourceAssembler.ToCommandFromResource)
            .ToList());
}
