namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the operations that the mobile application recorded offline.
/// </summary>
/// <param name="Operations">The recorded operations.</param>
public record MobileSyncBatchResource(IReadOnlyList<RegisterLivestockOperationResource>? Operations);
