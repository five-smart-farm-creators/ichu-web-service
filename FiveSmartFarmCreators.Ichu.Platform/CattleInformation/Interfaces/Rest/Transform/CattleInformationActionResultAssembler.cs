using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

/// <summary>
///     Turns a business error into a problem response. The status follows the error type, the title is the message in
///     the language of the request, and the stable code lets clients branch on the failure.
/// </summary>
public static class CattleInformationActionResultAssembler
{
    public static IActionResult ToProblem(ControllerBase controller, Error error, IStringLocalizer<CattleInformationMessages> localizer)
    {
        var statusCode = ToStatusCode(error.Type);
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = localizer[error.Code].Value,
            Instance = controller.HttpContext.Request.Path.Value
        };
        problem.Extensions["code"] = error.Code;
        if (error is DuplicateEarTagError duplicate)
        {
            problem.Extensions["conflictingCattleId"] = duplicate.ConflictingCattleId.Value;
        }

        return controller.StatusCode(statusCode, problem);
    }

    private static int ToStatusCode(EErrorType type) => type switch
    {
        EErrorType.Validation => StatusCodes.Status400BadRequest,
        EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        EErrorType.Forbidden => StatusCodes.Status403Forbidden,
        EErrorType.NotFound => StatusCodes.Status404NotFound,
        EErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
