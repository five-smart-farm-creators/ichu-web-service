namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing one page of alerts.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of alerts per page.</param>
/// <param name="TotalCount">The total number of matching alerts.</param>
/// <param name="Items">The alerts of the page.</param>
public record AlertPageResource(int Page, int PageSize, int TotalCount, IReadOnlyList<AlertResource> Items);
