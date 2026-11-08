using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class AlertPageResourceFromEntityAssembler
{
    public static AlertPageResource ToResourceFromEntity(PagedResult<MonitoringAlert> page) =>
        new(page.Page, page.PageSize, page.TotalCount, page.Items.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
