using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Exposes attention priorities across the current account ranch portfolio.
/// </summary>
[Route("api/v1/analytics/portfolio")]
public sealed class PortfolioController(IAnalyticsQueryService service) : AnalyticsControllerBase
{
    /// <summary>
    ///     Returns the current account ranches ordered by attention priority.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok((await service.HandleAsync(new GetRanchPortfolioQuery(), cancellationToken))
            .Select(RanchPortfolioEntryResourceFromValueObjectAssembler.ToResourceFromValueObject));
}
