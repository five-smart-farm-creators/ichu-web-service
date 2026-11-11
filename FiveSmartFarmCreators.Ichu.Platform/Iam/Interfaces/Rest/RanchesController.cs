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
    [EndpointDescription("Returns the ranch that a veterinarian identifies with the access code its manager shares, so that the veterinarian can request advisory access.")]
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
