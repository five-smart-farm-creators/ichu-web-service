using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Filters;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;

/// <summary>Exposes professional-profile reads and updates for the current account.</summary>
[ApiController]
[Route("api/v1/profiles")]
[Authorize(AuthenticationSchemes = "Firebase")]
[TypeFilter(typeof(IamExceptionFilter))]
/// <summary>Defines the Profiles Controller contract.</summary>
public sealed class ProfilesController(IProfessionalProfileCommandService commands, IAccountQueryService queries) : ControllerBase
{
    /// <summary>Returns the current account and its declared professional profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var account = await queries.HandleAsync(new GetCurrentAccountQuery(), cancellationToken);
        return Ok(AccountResourceFromEntityAssembler.ToResourceFromEntity(account));
    }

    /// <summary>Completes the current veterinarian profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> Complete(CompleteProfileResource resource, CancellationToken cancellationToken)
    {
        var command = CompleteProfessionalProfileCommandFromResourceAssembler.ToCommandFromResource(resource);
        var account = await commands.HandleAsync(command, cancellationToken);
        return Ok(AccountResourceFromEntityAssembler.ToResourceFromEntity(account));
    }
}
