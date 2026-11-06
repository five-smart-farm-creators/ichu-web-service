using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Filters;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;

/// <summary>Exposes advisory requests, history and owner decisions.</summary>
[ApiController]
[Route("api/v1/herd-advisories")]
[Authorize(AuthenticationSchemes = "Firebase")]
[TypeFilter(typeof(IamExceptionFilter))]
/// <summary>Defines the Herd Advisories Controller contract.</summary>
public sealed class HerdAdvisoriesController(IHerdAdvisoryCommandService commands, IHerdAdvisoryQueryService queries) : ControllerBase
{
    /// <summary>Requests access to a ranch for the current veterinarian.</summary>
    [HttpPost]
    public async Task<IActionResult> RequestAdvisory(RequestAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = RequestHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(resource);
        var advisory = await commands.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/herd-advisories?ranchId={advisory.RanchId}", AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }

    /// <summary>Lists advisory history visible to the authenticated account.</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid ranchId, CancellationToken cancellationToken)
    {
        var advisories = await queries.HandleAsync(new GetHerdAdvisoriesByRanchQuery(ranchId), cancellationToken);
        return Ok(advisories.Select(AdvisoryResourceFromDetailsAssembler.ToResourceFromDetails));
    }

    /// <summary>Grants, rejects or revokes a request as the ranch owner.</summary>
    [HttpPatch("{advisoryId:guid}")]
    public async Task<IActionResult> Decide(Guid advisoryId, DecideAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = DecideHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(advisoryId, resource);
        var advisory = await commands.HandleAsync(command, cancellationToken);
        return Ok(AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }

    /// <summary>Directly authorizes an eligible veterinarian as the ranch owner.</summary>
    [HttpPost("grants")]
    /// <summary>Grants a pending advisory with an explicit scope and future expiration.</summary>
    public async Task<IActionResult> Grant(GrantAdvisoryResource resource, CancellationToken cancellationToken)
    {
        var command = GrantHerdAdvisoryCommandFromResourceAssembler.ToCommandFromResource(resource);
        var advisory = await commands.HandleAsync(command, cancellationToken);
        return Created($"/api/v1/herd-advisories?ranchId={advisory.RanchId}", AdvisoryResourceFromEntityAssembler.ToResourceFromEntity(advisory));
    }
}
