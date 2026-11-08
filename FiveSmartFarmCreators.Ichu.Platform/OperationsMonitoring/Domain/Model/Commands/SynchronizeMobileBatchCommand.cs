namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Synchronizes the livestock operations that the mobile application recorded offline.
/// </summary>
public record SynchronizeMobileBatchCommand(IReadOnlyList<RegisterLivestockOperationCommand> Operations);
