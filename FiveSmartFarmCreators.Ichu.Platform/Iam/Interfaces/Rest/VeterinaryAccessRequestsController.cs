using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;

[ApiController]
[Route("api/v1/veterinary-access-requests")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Veterinary Access Requests")]
public sealed class VeterinaryAccessRequestsController(
    IHerdAdvisoryCommandService commands,
    IHerdAdvisoryQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Request veterinary access to a ranch")]
    [EndpointDescription("Only an authenticated veterinarian with a complete professional profile may submit a request. First find the ranch using GET /api/v1/ranches?accessCode=..., then submit its ranchId and a reason. The request starts as Pending and grants no ranch access. A pending request or active authorization for the same veterinarian and ranch returns 409.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestAsync(RequestVeterinaryAccessResource resource, CancellationToken cancellationToken)
    {
        var command = new RequestHerdAdvisoryCommand(resource.RanchId!.Value, resource.Reason);
        var result = await commands.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Created($"/api/v1/veterinary-access-requests?ranchId={result.Value!.RanchId}",
            AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpGet]
    [EndpointSummary("List veterinary access requests for a ranch")]
    [EndpointDescription("Requires ranchId. The authenticated ranch owner sees all requests and authorizations for that ranch, including the veterinarian's professional profile. A veterinarian sees only their own requests, including pending, rejected, revoked and expired entries. Other accounts cannot list another owner's ranch requests.")]
    [ProducesResponseType<IReadOnlyList<AdvisoryResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery, BindRequired] Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queries.HandleAsync(new GetHerdAdvisoriesByRanchQuery(ranchId), cancellationToken);
        return result.IsFailure
            ? IamActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(result.Value!.Select(AdvisoryResourceFromDetailsAssembler.ToResourceFromDetails));
    }

    [HttpPatch("{requestId:guid}")]
    [EndpointSummary("Decide a veterinary access request as the ranch owner")]
    [EndpointDescription("Only the authenticated owner of the request's ranch may decide it. Use decision 'grant' with a future expiresAt and scope 'Read' or 'ClinicalWrite' to approve a pending request; an omitted scope defaults to Read. Use 'reject' to reject a pending request or 'revoke' to end active access. The server resolves the ranch from requestId. Veterinarians and other ranch owners cannot decide the request.")]
    [ProducesResponseType<AdvisoryResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DecideAsync(Guid requestId, DecideAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = DecideHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(null, requestId, resource);
        if (command.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await commands.HandleAsync(command.Value!, cancellationToken);
        return result.IsFailure
            ? IamActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
