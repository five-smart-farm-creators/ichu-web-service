using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Filters;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;

/// <summary>Exposes registration, sign-in and current-account queries.</summary>
[ApiController]
[Route("api/v1/accounts")]
[Authorize(AuthenticationSchemes = "Firebase")]
[TypeFilter(typeof(IamExceptionFilter))]
/// <summary>Defines the Accounts Controller contract.</summary>
public sealed class AccountsController(IAccountCommandService commands, IAccountQueryService queries) : ControllerBase
{
    /// <summary>Registers an application account for the verified Firebase identity.</summary>
    [HttpPost]
    public async Task<IActionResult> Register(RegisterAccountResource resource, CancellationToken cancellationToken)
    {
        var command = RegisterAccountCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commands.HandleAsync(command, cancellationToken);
        return Created("/api/v1/accounts/me", RegisteredAccountResourceFromResultAssembler.ToResourceFromResult(result));
    }

    /// <summary>Authenticates credentials through Firebase and applies local account lockout.</summary>
    [HttpPost("sessions")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(SignInResource resource, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
        var token = await commands.HandleAsync(command, cancellationToken);
        return Ok(new AuthenticatedAccountResource(token, "Bearer"));
    }

    /// <summary>Returns the authenticated application's account.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var account = await queries.HandleAsync(new GetCurrentAccountQuery(), cancellationToken);
        return Ok(AccountResourceFromEntityAssembler.ToResourceFromEntity(account));
    }

    /// <summary>Lists the current account's effective ranch access.</summary>
    [HttpGet("me/ranches")]
    public async Task<IActionResult> GetRanches(CancellationToken cancellationToken)
    {
        var access = await queries.HandleAsync(new GetCurrentRanchesQuery(), cancellationToken);
        return Ok(access.Select(RanchAccessResourceFromProjectionAssembler.ToResourceFromProjection));
    }
}
