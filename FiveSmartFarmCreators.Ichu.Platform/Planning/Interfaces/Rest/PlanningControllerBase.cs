using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
/// Translates expected Planning failures into HTTP problem responses.
/// </summary>
[ApiController]
[Authorize]
public abstract class PlanningControllerBase : ControllerBase
{
    /// <summary>
    /// Executes a Planning operation and maps known failures to API errors.
    /// </summary>
    /// <typeparam name="T">The success resource type.</typeparam>
    /// <param name="operation">The asynchronous application operation.</param>
    /// <returns>The operation result or an RFC 9457 problem response.</returns>
    protected async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        return await ExecuteAsync(operation, StatusCodes.Status200OK, null);
    }

    /// <summary>
    /// Executes a resource-creating Planning operation and maps known failures to API errors.
    /// </summary>
    /// <typeparam name="T">The created resource type.</typeparam>
    /// <param name="operation">The asynchronous application operation.</param>
    /// <param name="location">Builds the URI of the created resource.</param>
    /// <returns>The created resource with HTTP 201 or an RFC 9457 problem response.</returns>
    protected async Task<ActionResult<T>> ExecuteCreatedAsync<T>(
        Func<Task<T>> operation,
        Func<T, string> location)
    {
        return await ExecuteAsync(operation, StatusCodes.Status201Created, location);
    }

    private async Task<ActionResult<T>> ExecuteAsync<T>(
        Func<Task<T>> operation,
        int successStatusCode,
        Func<T, string>? location)
    {
        try
        {
            var result = await operation();
            return location is null
                ? StatusCode(successStatusCode, result)
                : Created(location(result), result);
        }
        catch (PlanningDomainException exception)
        {
            return ProblemResponse(StatusCodes.Status400BadRequest, "Invalid Planning request", exception.Code, exception.Message);
        }
        catch (PlanningRequestException exception)
        {
            var statusCode = exception.Kind switch
            {
                PlanningFailureKind.NotFound => StatusCodes.Status404NotFound,
                PlanningFailureKind.Conflict => StatusCodes.Status409Conflict,
                PlanningFailureKind.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            return ProblemResponse(statusCode, "Planning request failed", exception.Code, exception.Message);
        }
    }

    private ObjectResult ProblemResponse(int statusCode, string title, string code, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
        problem.Extensions["code"] = code;
        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
