using System.Text.Json.Serialization;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Interfaces.AspNetCore.OpenApi;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Interfaces.AspNetCore.OpenApi;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Microsoft.EntityFrameworkCore;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Seeding;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    // Enums travel as names in both directions; integers are still accepted in requests for existing clients.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddLocalization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<EdgeGatewayKeyHeaderTransformer>();
    options.AddOperationTransformer<StripeWebhookSecurityTransformer>();
});
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
    {
        throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
    }

    // Credentials stay out of source control: the template references environment variables such as %ICHU_DB_PASSWORD%.
    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
    options.UseNpgsql(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .AddInterceptors(serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Modules that depend on the base DbContext type receive the shared application context.
builder.Services.AddScoped<DbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DispatchDomainEventsInterceptor>();

// Command pipeline behaviors and event handler discovery are shared by every bounded context.
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));
builder.Services.AddCortexMediator([typeof(Program)]);

// The Firebase project identifier and Web API key come from configuration, never from source control.
builder.Services.AddIam(builder.Configuration);
builder.Services.AddSubscriptionPlans(builder.Configuration);
builder.Services.AddCattleInformation();
builder.Services.AddIoTAssets(builder.Configuration);

builder.Services.AddOperationsMonitoring(builder.Configuration);
builder.Services.AddPlanning();
builder.Services.AddDashboardAnalytics(builder.Configuration);

var app = builder.Build();

// Pending migrations are applied on startup; the call does nothing when the schema is already up to date.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    // The seeders run in dependency order, and each one skips the records that already exist.
    using var seedScope = app.Services.CreateScope();
    await seedScope.ServiceProvider.GetRequiredService<DevelopmentIamSeeder>()
        .SeedAsync(CancellationToken.None);
    await seedScope.ServiceProvider.GetRequiredService<DevelopmentSubscriptionSeeder>()
        .SeedAsync(CancellationToken.None);
    await seedScope.ServiceProvider.GetRequiredService<DevelopmentCattleSeeder>()
        .SeedAsync(CancellationToken.None);
    await seedScope.ServiceProvider.GetRequiredService<DevelopmentIoTAssetsSeeder>()
        .SeedAsync(CancellationToken.None);
    await seedScope.ServiceProvider.GetRequiredService<DevelopmentMonitoringRulesSeeder>()
        .SeedAsync(CancellationToken.None);
}

app.UseExceptionHandler();

// English is the default; Spanish is served when the request asks for it through Accept-Language.
var supportedCultures = new[] { "en", "es" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    // The built-in document declares the bearer and gateway key schemes, which the Swashbuckle generator omits.
    // Try it out is enabled so that a developer can call the API from Swagger UI with the demo token.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ICHU API v1");
        options.EnableTryItOutByDefault();
    });
}

app.UseCors("AllowAllPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
