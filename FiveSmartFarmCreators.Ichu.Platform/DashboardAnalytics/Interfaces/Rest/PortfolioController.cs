using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

[ApiController]
[Authorize(AuthenticationSchemes = "Firebase")]
[Route("api/v1/analytics/portfolio")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class PortfolioController(
    IAnalyticsQueryService service,
    IStringLocalizer<DashboardAnalyticsMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List the attention priorities of the accessible ranches")]
    [EndpointDescription("Returns the ranches of the current account, ordered by their open critical, warning and treatment alerts.")]
    [ProducesResponseType<IReadOnlyList<RanchPortfolioEntryResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await service.HandleAsync(new GetRanchPortfolioQuery(), cancellationToken);
        return result.IsFailure
            ? DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(result.Value!.Select(RanchPortfolioEntryResourceFromValueObjectAssembler.ToResourceFromValueObject).ToList());
    }
}
