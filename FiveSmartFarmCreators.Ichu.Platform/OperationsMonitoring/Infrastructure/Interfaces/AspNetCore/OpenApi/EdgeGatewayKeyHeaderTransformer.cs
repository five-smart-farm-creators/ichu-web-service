using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Documents the required X-Gateway-Key header on the Edge Gateway operations so that API clients such as Swagger
///     UI can send the key of a simulated gateway.
/// </summary>
public class EdgeGatewayKeyHeaderTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    ///     Adds the gateway key header parameter when the operation is an Edge Gateway endpoint.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to transform.</param>
    /// <param name="context">The operation transformer context.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the operation is updated.</returns>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (!IsEdgeGatewayOperation(context.Description.ActionDescriptor as ControllerActionDescriptor))
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = SimulatedEdgeGatewayAuthenticator.KeyHeader,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Edge Gateway key. In Development, use the Key of a gateway in OperationsMonitoring:Simulation:Gateways of appsettings.Development.json.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
        return Task.CompletedTask;
    }

    private static bool IsEdgeGatewayOperation(ControllerActionDescriptor? action) =>
        action is not null &&
        (action.ControllerTypeInfo.AsType() == typeof(TelemetryController) && action.MethodInfo.Name == nameof(TelemetryController.SynchronizeBatch) ||
         action.ControllerTypeInfo.AsType() == typeof(EdgeGatewaysController) && action.MethodInfo.Name == nameof(EdgeGatewaysController.GetMonitoringRules));
}
