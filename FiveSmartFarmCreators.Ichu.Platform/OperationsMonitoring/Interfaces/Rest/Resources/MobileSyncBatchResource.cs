namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record MobileSyncBatchResource(IReadOnlyList<RegisterLivestockOperationResource>? Operations);
