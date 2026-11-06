namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to synchronize the livestock operations that the mobile application recorded offline.
/// </summary>
/// <param name="Operations">The recorded operations.</param>
public sealed record SynchronizeMobileBatchCommand(IReadOnlyList<RegisterLivestockOperationCommand> Operations);
