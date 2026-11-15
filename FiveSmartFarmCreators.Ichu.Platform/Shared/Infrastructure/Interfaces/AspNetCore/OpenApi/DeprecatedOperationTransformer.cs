using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Marks compatibility actions as deprecated in the OpenAPI document while keeping their routes available.
/// </summary>
public sealed class DeprecatedOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor action
            && action.MethodInfo.IsDefined(typeof(ObsoleteAttribute), inherit: true))
        {
            operation.Deprecated = true;
        }

        return Task.CompletedTask;
    }
}
