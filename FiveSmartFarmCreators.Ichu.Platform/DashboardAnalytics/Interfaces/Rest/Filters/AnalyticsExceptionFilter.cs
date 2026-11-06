using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Filters;

/// <summary>
///     Maps recognized analytics and request failures to HTTP problem responses.
/// </summary>
public sealed class AnalyticsExceptionFilter : IExceptionFilter
{
    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            ArgumentException => 400,
            AnalyticsException { Code: "forbidden" } => 403,
            AnalyticsException { Code: "no_data" } => 422,
            AnalyticsException { Code: "unavailable" } => 503,
            AnalyticsException { Code: "unauthorized" } => 401,
            _ => 0
        };
        if (status == 0)
        {
            return;
        }
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = context.Exception.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
