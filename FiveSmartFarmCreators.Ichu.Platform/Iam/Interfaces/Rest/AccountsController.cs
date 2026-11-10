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
[Route("api/v1/accounts")]
[Authorize(AuthenticationSchemes = "Firebase")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class AccountsController(
    IAccountCommandService commands,
    IAccountQueryService queries,
    IStringLocalizer<IamMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Register an application account")]
    [EndpointDescription("Creates a pending account for the verified identity and, for a ranch manager, the ranch it owns.")]
    [ProducesResponseType<RegisteredAccountResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(RegisterAccountResource resource, CancellationToken cancellationToken)
    {
        var command = RegisterAccountCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, command.DomainError, localizer);
        }

        var result = await commands.HandleAsync(command.Value!, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Created("/api/v1/accounts/me", RegisteredAccountResourceFromResultAssembler.ToResourceFromResult(result.Value!));
    }

    [HttpPost("sessions")]
    [AllowAnonymous]
    [EndpointSummary("Sign in with email and password")]
    [EndpointDescription("Authenticates the credentials through the identity provider and locks the account after repeated failures.")]
    [ProducesResponseType<AuthenticatedAccountResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SignInAsync(SignInResource resource, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commands.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(new AuthenticatedAccountResource(result.Value!, "Bearer"));
    }

    [HttpGet("me")]
    [EndpointSummary("Get the authenticated account")]
    [EndpointDescription("Returns the account of the verified identity. The request activates a verified pending account.")]
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

    [HttpGet("me/ranches")]
    [EndpointSummary("List the ranches the account may access")]
    [EndpointDescription("Returns the effective scope and optional expiration of each ranch of the authenticated account.")]
    [ProducesResponseType<IReadOnlyList<RanchAccessResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRanchesAsync(CancellationToken cancellationToken)
    {
        var access = await queries.HandleAsync(new GetCurrentRanchesQuery(), cancellationToken);
        if (access.IsFailure)
        {
            return IamActionResultAssembler.ToProblem(this, access.DomainError, localizer);
        }

        return Ok(access.Value!.Select(RanchAccessResourceFromProjectionAssembler.ToResourceFromProjection));
    }
}
