using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from pages of <see cref="MonitoringAlert"/> aggregates.
/// </summary>
public static class AlertPageResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a page of alerts into an <see cref="AlertPageResource"/>.
    /// </summary>
    /// <param name="page">The page of alert aggregates.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static AlertPageResource ToResourceFromEntity(PagedResult<MonitoringAlert> page) =>
        new(page.Page, page.PageSize, page.TotalCount, page.Items.Select(AlertResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
