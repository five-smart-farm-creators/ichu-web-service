using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

[ApiController]
[Authorize(AuthenticationSchemes = "Firebase")]
[Route("api/v1/ranches/{ranchId:guid}/dashboard")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class DashboardController(
    IAnalyticsQueryService service,
    IStringLocalizer<DashboardAnalyticsMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get herd indicators for a ranch period")]
    [EndpointDescription("Returns the herd indicators of the period with their observation coverage and source freshness.")]
    [ProducesResponseType<HerdIndicatorSetResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(Guid ranchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var period = DateRange.Create(from, to);
        if (period.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await service.HandleAsync(new GetHerdIndicatorsQuery(ranchId, period.Value!), cancellationToken);
        return result.IsFailure
            ? DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(HerdIndicatorSetResourceFromValueObjectAssembler.ToResourceFromValueObject(result.Value!));
    }
}
