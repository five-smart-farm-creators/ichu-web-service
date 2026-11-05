using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record AlertPageResource(int Page, int PageSize, int TotalCount, IReadOnlyList<AlertResource> Items)
{
    public static AlertPageResource From(PagedResult<MonitoringAlert> page) =>
        new(page.Page, page.PageSize, page.TotalCount, page.Items.Select(AlertResource.From).ToList());
}
