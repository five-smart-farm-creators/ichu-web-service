using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

/// <summary>
///     Maps a successful result to its response and a business error to a problem response with a localized title.
///     A stopped integration is reported as unavailable so that clients retry instead of treating the request as invalid.
/// </summary>
public static class IoTAssetsActionResultAssembler
{
    public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result,
        IStringLocalizer<IoTAssetsMessages> localizer, Func<T, IActionResult> successAction)
    {
        if (result.IsSuccess)
        {
            return successAction(result.Value!);
        }

        var error = result.DomainError;
        var status = error.Type switch
        {
            EErrorType.Validation => StatusCodes.Status400BadRequest,
            EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            EErrorType.Forbidden => StatusCodes.Status403Forbidden,
            EErrorType.NotFound => StatusCodes.Status404NotFound,
            EErrorType.Conflict => StatusCodes.Status409Conflict,
            _ when error.Code == IoTAssetsErrors.IntegrationUnavailable.Code => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
        var problem = new ProblemDetails
        {
            Status = status,
            Title = localizer[error.Code].Value,
            Instance = controller.HttpContext.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        var response = new ObjectResult(problem) { StatusCode = status };
        response.ContentTypes.Add("application/problem+json");
        return response;
    }
}
