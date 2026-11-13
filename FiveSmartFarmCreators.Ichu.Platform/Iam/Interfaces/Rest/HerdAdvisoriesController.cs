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
[Route("api/v1/ranches/{ranchId:guid}/herd-advisories")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Veterinary Access Requests")]
public sealed class HerdAdvisoriesController(
    IHerdAdvisoryCommandService commands,
    IHerdAdvisoryQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Request ranch access as a veterinarian")]
    [EndpointDescription("Compatibility route for POST /api/v1/veterinary-access-requests. Only an authenticated veterinarian with a complete profile can request access. The request starts as Pending, grants no access and returns 409 when a pending request or active authorization already exists.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestAsync(Guid ranchId, RequestAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = RequestHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        var result = await commands.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var advisory = result.Value!;
        return Created($"/api/v1/ranches/{ranchId}/herd-advisories",
            AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }

    [HttpGet]
    [EndpointSummary("List veterinary access requests through the ranch route")]
    [EndpointDescription("Compatibility route for GET /api/v1/veterinary-access-requests?ranchId=.... The authenticated ranch owner sees every request and authorization for the ranch, with the professional profile. Veterinarians see only their own requests and authorizations, including pending, rejected, revoked and expired entries.")]
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
    [EndpointSummary("Decide veterinary access through the ranch route")]
    [EndpointDescription("Compatibility route for PATCH /api/v1/veterinary-access-requests/{requestId}. Only the authenticated ranch owner may grant or reject a pending request, or revoke active access. Granting requires a future expiresAt and scope Read or ClinicalWrite; an omitted scope defaults to Read.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DecideAsync(Guid ranchId, Guid advisoryId, DecideAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = DecideHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(ranchId, advisoryId, resource);
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
    [EndpointDescription("Separate owner-initiated authorization flow retained for US-04. Only the authenticated ranch owner may authorize an active veterinarian with a complete profile, a future expiresAt and scope Read or ClinicalWrite, without a prior request. An existing pending request is granted instead of creating another one; active access returns 409. To decide a veterinarian's request from US-39, use PATCH /api/v1/veterinary-access-requests/{requestId}.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GrantAsync(Guid ranchId, GrantAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = GrantHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
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
        return Created($"/api/v1/ranches/{ranchId}/herd-advisories",
            AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }
}
