using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

public sealed class OperationsMonitoringExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not MonitoringException error)
        {
            return;
        }

        var status = error.Kind switch
        {
            MonitoringErrorKind.InvalidInput => StatusCodes.Status400BadRequest,
            MonitoringErrorKind.Unauthenticated => StatusCodes.Status401Unauthorized,
            MonitoringErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            MonitoringErrorKind.NotFound => StatusCodes.Status404NotFound,
            MonitoringErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = error.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
