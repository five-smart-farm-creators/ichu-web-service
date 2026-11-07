using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest;

/// <summary>
/// Maps known module failures to Problem Details while leaving unrelated exceptions to the host pipeline.
/// </summary>
public sealed class CattleInformationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not CattleInformationException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            "invalid_input" => StatusCodes.Status400BadRequest,
            "invalid_state" or "duplicate_ear_tag" or "conflict" => StatusCodes.Status409Conflict,
            "not_found" => StatusCodes.Status404NotFound,
            "forbidden" => StatusCodes.Status403Forbidden,
            "unauthenticated" => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "The cattle information request could not be completed.",
            Detail = exception.Message,
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["code"] = exception.Code;
        if (exception.Code == "duplicate_ear_tag" && exception.ConflictingCattleId.HasValue)
        {
            problem.Extensions["conflictingCattleId"] = exception.ConflictingCattleId.Value;
        }

        context.Result = new ObjectResult(problem) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}
