using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Exposes executive report downloads subject to ranch access and subscription entitlement.
/// </summary>
[Route("api/v1/ranches/{ranchId:guid}/reports")]
public sealed class ReportsController(IExecutiveReportQueryService service) : AnalyticsControllerBase
{
    /// <summary>
    ///     Downloads a subscription-authorized report without caching the generated file.
    /// </summary>
    [HttpGet("executive")]
    public async Task<IActionResult> Export(Guid ranchId, DateTimeOffset from, DateTimeOffset to, string format, CancellationToken cancellationToken)
    {
        var report = await service.HandleAsync(new GetExecutiveReportQuery(ranchId, new DateRange(from, to), format), cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return File(report.Content, report.ContentType, report.FileName);
    }
}
