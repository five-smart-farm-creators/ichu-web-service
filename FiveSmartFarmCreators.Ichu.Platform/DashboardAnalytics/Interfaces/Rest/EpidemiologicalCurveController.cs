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
[Route("api/v1/ranches/{ranchId:guid}/epidemiological-curves")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class EpidemiologicalCurveController(
    IAnalyticsQueryService service,
    IStringLocalizer<DashboardAnalyticsMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get the daily case progression of a health condition")]
    [EndpointDescription("Returns the daily case onset counts and the incubation assessment that the observation window supports.")]
    [ProducesResponseType<EpidemiologicalCurveResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(Guid ranchId, DateTimeOffset from, DateTimeOffset to, string condition, CancellationToken cancellationToken)
    {
        var period = DateRange.Create(from, to);
        if (period.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await service.HandleAsync(new GetEpidemiologicalCurveQuery(ranchId, period.Value!, condition), cancellationToken);
        return result.IsFailure
            ? DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(EpidemiologicalCurveResourceFromValueObjectAssembler.ToResourceFromValueObject(result.Value!));
    }
}
