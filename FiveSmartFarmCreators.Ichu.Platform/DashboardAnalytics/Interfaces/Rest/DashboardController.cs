using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Exposes herd indicators for authorized ranch observation windows.
/// </summary>
[Route("api/v1/ranches/{ranchId:guid}/dashboard")]
public sealed class DashboardController(IAnalyticsQueryService service) : AnalyticsControllerBase
{
    /// <summary>
    ///     Returns herd indicators with observation coverage and source freshness.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        Ok(HerdIndicatorSetResourceFromValueObjectAssembler.ToResourceFromValueObject(
            await service.HandleAsync(new GetHerdIndicatorsQuery(ranchId, new DateRange(from, to)), cancellationToken)));
}
