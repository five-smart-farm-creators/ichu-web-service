namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record WaterTroughStatusesResource(Guid RanchId, IReadOnlyList<WaterTroughStatusResource> Items);
