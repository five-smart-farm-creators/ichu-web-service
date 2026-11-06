using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Configuration.Extensions;

/// <summary>Registers the IAM application services and infrastructure adapters.</summary>
public static class IamServiceCollectionExtensions
{
    /// <summary>Registers IAM application contracts, persistence adapters and Firebase authentication.</summary>
    public static IServiceCollection AddIam(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddHttpClient<IIdentityProvider, FirebaseAuthenticationAcl>(client => client.Timeout = TimeSpan.FromSeconds(15))
            .RemoveAllLoggers();
        services.AddAuthentication(FirebaseAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(FirebaseAuthenticationHandler.SchemeName, _ => { });
        services.AddAuthorization();
        services.AddScoped<IIamRepository, IdentityRepository>();
        services.AddScoped<ICurrentAccount, HttpCurrentAccount>();
        services.AddScoped<IIamContextFacade, IamContextFacade>();
        services.AddScoped<AuthorizationDomainService>();
        services.AddScoped<IAccountCommandService, AccountCommandService>();
        services.AddScoped<IProfessionalProfileCommandService, ProfessionalProfileCommandService>();
        services.AddScoped<IHerdAdvisoryCommandService, HerdAdvisoryCommandService>();
        services.AddScoped<IAccountQueryService, AccountQueryService>();
        services.AddScoped<IHerdAdvisoryQueryService, HerdAdvisoryQueryService>();
        services.AddScoped<IAuthorizationQueryService, AuthorizationQueryService>();
        services.AddScoped<IamExceptionFilter>();
        return services;
    }
}
