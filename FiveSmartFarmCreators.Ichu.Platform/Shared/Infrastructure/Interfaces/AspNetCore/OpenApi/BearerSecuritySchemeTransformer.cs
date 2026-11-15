using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;

/// <summary>
///     Adds the bearer security scheme to the OpenAPI document so that API clients such as Swagger UI can send the
///     Firebase identity token.
/// </summary>
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string SchemeName = "Bearer";

    /// <summary>
    ///     Declares the bearer scheme and requires it at document level.
    /// </summary>
    /// <param name="document">The OpenAPI document to transform.</param>
    /// <param name="context">The document transformer context.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the document is updated.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Description = "Firebase identity token returned by the sign-in endpoint."
        };

        // Authenticated operations inherit this requirement; anonymous operations override it with an empty list.
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
        });
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Keeps public catalog, sign-in, Stripe and gateway operations free of the user bearer requirement.
    /// </summary>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            operation.Security = [];
        }

        return Task.CompletedTask;
    }
}
