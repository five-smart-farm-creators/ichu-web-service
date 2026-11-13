using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;

[ApiController]
[Route("api/v1/ranches")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class RanchesController(
    IRanchQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Find a ranch by its access code")]
    [EndpointDescription("Only an authenticated veterinarian may look up a ranch using the access code shared by its manager. This lookup grants no access. A veterinarian with a complete professional profile can then submit the returned ranchId and a reason to POST /api/v1/veterinary-access-requests and wait for the ranch owner's decision.")]
    [ProducesResponseType<RanchSummaryResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupAsync([FromQuery] string accessCode, CancellationToken cancellationToken)
    {
        var result = await queries.HandleAsync(new GetRanchByAccessCodeQuery(accessCode), cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(RanchSummaryResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
