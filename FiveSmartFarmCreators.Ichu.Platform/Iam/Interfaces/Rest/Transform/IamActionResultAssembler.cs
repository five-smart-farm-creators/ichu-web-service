using FiveSmartFarmCreators.Ichu.Platform.Iam.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Turns a business error into a problem response. The status follows the error type, the title is the localized message, and the stable code lets clients branch on the failure.
/// </summary>
public static class IamActionResultAssembler
{
    public static IActionResult ToProblem(ControllerBase controller, Error error, IStringLocalizer<IamMessages> localizer)
    {
        var statusCode = ToStatusCode(error.Type);
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = localizer[error.Code].Value,
            Instance = controller.HttpContext.Request.Path.Value
        };
        problem.Extensions["code"] = error.Code;
        return controller.StatusCode(statusCode, problem);
    }

    private static int ToStatusCode(EErrorType type) => type switch
    {
        EErrorType.Validation => StatusCodes.Status400BadRequest,
        EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        EErrorType.Forbidden => StatusCodes.Status403Forbidden,
        EErrorType.NotFound => StatusCodes.Status404NotFound,
        EErrorType.Conflict => StatusCodes.Status409Conflict,
        EErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };
}
