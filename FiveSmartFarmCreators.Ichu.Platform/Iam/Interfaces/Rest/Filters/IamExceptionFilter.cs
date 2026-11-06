using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Filters;

/// <summary>Defines the Iam Exception Filter contract.</summary>
public sealed class IamExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            IamException { Code: "not_found" } => 404,
            IamException { Code: "conflict" } => 409,
            IamException { Code: "forbidden" } => 403,
            IamException { Code: "unauthorized" or "invalid_credentials" } => 401,
            IamException { Code: "unavailable" } => 503,
            ArgumentException => 400,
            UnauthorizedAccessException => 403,
            _ => 0
        };
        if (status == 0) return;
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = context.Exception.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
