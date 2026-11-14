using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Integration;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Documents the required X-Gateway-Key header on the Edge Gateway operations so that API clients such as Swagger UI can send the gateway key.
/// </summary>
public class EdgeGatewayKeyHeaderTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    ///     Adds the gateway key header parameter when the operation is an Edge Gateway endpoint.
    /// </summary>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (!IsEdgeGatewayOperation(context.Description.ActionDescriptor as ControllerActionDescriptor))
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = IoTAssetsEdgeGatewayAuthenticator.KeyHeader,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Edge Gateway key in the form {gatewayId}.{secret}, returned once when the gateway is registered in IoT Assets.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
        return Task.CompletedTask;
    }

    private static bool IsEdgeGatewayOperation(ControllerActionDescriptor? action) =>
        action is not null &&
        (action.ControllerTypeInfo.AsType() == typeof(TelemetryController) && action.MethodInfo.Name == nameof(TelemetryController.SynchronizeBatch) ||
         action.ControllerTypeInfo.AsType() == typeof(WaterControllersController) && action.MethodInfo.Name == nameof(WaterControllersController.ReceiveReading) ||
         action.ControllerTypeInfo.AsType() == typeof(EdgeGatewaysController) && (action.MethodInfo.Name == nameof(EdgeGatewaysController.GetMonitoringRules) ||
             action.MethodInfo.Name == nameof(EdgeGatewaysController.GetDeviceAssignments)));
}
