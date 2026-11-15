namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record WaterControllerReadingsPageResource(
    Guid ControllerId,
    DateTimeOffset From,
    DateTimeOffset To,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<WaterTroughReadingResource> Items);
