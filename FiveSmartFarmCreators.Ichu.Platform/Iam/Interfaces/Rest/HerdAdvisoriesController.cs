using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
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
[Route("api/v1/herd-advisories")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class HerdAdvisoriesController(
    IHerdAdvisoryCommandService commands,
    IHerdAdvisoryQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Request ranch access as a veterinarian")]
    [EndpointDescription("Creates a pending request for a ranch, when the veterinarian has a complete profile and no open request or active advisory for it.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestAsync(RequestAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = RequestHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commands.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var advisory = result.Value!;
        return Created($"/api/v1/herd-advisories?ranchId={advisory.RanchId}",
            AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }

    [HttpGet]
    [EndpointSummary("List the advisories of a ranch")]
    [EndpointDescription("Returns the advisory history of the ranch that the authenticated account may see.")]
    [ProducesResponseType<IReadOnlyList<AdvisoryResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queries.HandleAsync(new GetHerdAdvisoriesByRanchQuery(ranchId), cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(AdvisoryResourceFromDetailsAssembler.ToResourceFromDetails));
    }

    [HttpPatch("{advisoryId:guid}")]
    [EndpointSummary("Decide a ranch advisory as the owner")]
    [EndpointDescription("Grants, rejects or revokes an advisory of a ranch that the authenticated account manages.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DecideAsync(Guid advisoryId, DecideAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = DecideHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(advisoryId, resource);
        if (command.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await commands.HandleAsync(command.Value!, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpPost("grants")]
    [EndpointSummary("Grant ranch access directly as the owner")]
    [EndpointDescription("Authorizes an eligible veterinarian over a ranch that the authenticated account manages, without a prior request.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GrantAsync(GrantAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = GrantHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await commands.HandleAsync(command.Value!, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var advisory = result.Value!;
        return Created($"/api/v1/herd-advisories?ranchId={advisory.RanchId}",
            AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }
}
