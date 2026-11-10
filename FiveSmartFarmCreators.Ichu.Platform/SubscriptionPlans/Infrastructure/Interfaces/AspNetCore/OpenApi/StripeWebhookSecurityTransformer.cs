using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Removes the document-wide bearer requirement from the Stripe webhook, which proves its origin with the event signature.
/// </summary>
public class StripeWebhookSecurityTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    ///     Clears the security requirements of the webhook operation, so that clients do not send a bearer token to it.
    /// </summary>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor action
            && action.ControllerTypeInfo.AsType() == typeof(StripeWebhooksController))
        {
            operation.Security = [];
        }

        return Task.CompletedTask;
    }
}
