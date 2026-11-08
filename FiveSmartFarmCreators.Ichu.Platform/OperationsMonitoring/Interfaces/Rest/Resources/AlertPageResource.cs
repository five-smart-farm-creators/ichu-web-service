namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record AlertPageResource(int Page, int PageSize, int TotalCount, IReadOnlyList<AlertResource> Items);
