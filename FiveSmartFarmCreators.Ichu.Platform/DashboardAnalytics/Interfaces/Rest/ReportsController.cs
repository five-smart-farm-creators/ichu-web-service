using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

[ApiController]
[Authorize(AuthenticationSchemes = "Firebase")]
[Route("api/v1/ranches/{ranchId:guid}/reports")]
[Produces(MediaTypeNames.Application.Json, "text/csv")]
public sealed class ReportsController(
    IExecutiveReportQueryService service,
    IStringLocalizer<DashboardAnalyticsMessages> localizer) : ControllerBase
{
    [HttpGet("executive")]
    [EndpointSummary("Download an executive report")]
    [EndpointDescription("Downloads the herd indicators of the period as JSON or CSV when the ranch subscription includes report exports. The file is never cached.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Export(Guid ranchId, DateTimeOffset from, DateTimeOffset to, string format, CancellationToken cancellationToken)
    {
        var period = DateRange.Create(from, to);
        if (period.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await service.HandleAsync(new GetExecutiveReportQuery(ranchId, period.Value!, format), cancellationToken);
        if (result.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var report = result.Value!;
        Response.Headers.CacheControl = "no-store";
        return File(report.Content, report.ContentType, report.FileName);
    }
}
