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
[Route("api/v1/profiles")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class ProfilesController(
    IProfessionalProfileCommandService commands,
    IAccountQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpGet("me")]
    [EndpointSummary("Get the authenticated account's profile")]
    [EndpointDescription("Returns the account of the verified identity with its professional profile, when it has one.")]
    [ProducesResponseType<AccountResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var account = await queries.HandleAsync(new GetCurrentAccountQuery(), cancellationToken);
        if (account.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, account.DomainError, localizer);
        }

        return Ok(AccountResourceFromEntityAssembler.ToResourceFromEntity(account.Value!));
    }

    [HttpPut("me")]
    [EndpointSummary("Complete the veterinarian profile")]
    [EndpointDescription("Declares the professional details of a veterinarian. Each license number belongs to one account.")]
    [ProducesResponseType<AccountResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteAsync(CompleteProfileResource resource, CancellationToken cancellationToken)
    {
        var command = CompleteProfessionalProfileCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commands.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(AccountResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
