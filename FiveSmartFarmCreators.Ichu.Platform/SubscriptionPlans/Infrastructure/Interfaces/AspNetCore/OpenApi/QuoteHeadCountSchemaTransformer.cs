using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Interfaces.AspNetCore.OpenApi;

public sealed class QuoteHeadCountSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.ParameterDescription is { Name: "headCount", Source: var source }
            && source == BindingSource.Query
            && context.JsonTypeInfo.Type == typeof(int))
        {
            // Swagger UI can reject a supplied query value when the schema lists both integer and string.
            schema.Type = JsonSchemaType.Integer;
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}
