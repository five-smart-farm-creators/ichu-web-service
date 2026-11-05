using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;

public sealed class SubscriptionPlansExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not SubscriptionException error)
        {
            return;
        }

        var status = error.Kind switch
        {
            SubscriptionErrorKind.InvalidInput => StatusCodes.Status400BadRequest,
            SubscriptionErrorKind.Unauthenticated => StatusCodes.Status401Unauthorized,
            SubscriptionErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            SubscriptionErrorKind.NotFound => StatusCodes.Status404NotFound,
            SubscriptionErrorKind.Conflict => StatusCodes.Status409Conflict,
            SubscriptionErrorKind.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = error.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
