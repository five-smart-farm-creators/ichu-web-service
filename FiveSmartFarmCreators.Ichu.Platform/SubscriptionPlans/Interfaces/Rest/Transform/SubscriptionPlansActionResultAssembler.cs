using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

/// <summary>
///     Turns a business error into a problem response. The status follows the error type, and the title is the message
///     in the language of the request; the stable code is added as an extension for clients that branch on it.
/// </summary>
public static class SubscriptionPlansActionResultAssembler
{
    public static IActionResult ToProblem(ControllerBase controller, Error error, IStringLocalizer<SubscriptionPlansMessages> localizer)
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
