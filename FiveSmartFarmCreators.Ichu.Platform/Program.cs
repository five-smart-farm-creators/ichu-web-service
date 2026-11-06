using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

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
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Modules that depend on the base DbContext type receive the shared application context.
builder.Services.AddScoped<DbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// The Firebase project identifier and Web API key come from configuration, never from source control.
builder.Services.AddIam(builder.Configuration);

builder.Services.AddOperationsMonitoring(builder.Configuration);
if (builder.Environment.IsDevelopment())
{
    // Simulated cattle, collars and gateways until the IoT Assets and Cattle Information facades replace them.
    builder.Services.AddOperationsMonitoringSimulation(builder.Configuration);
}

var app = builder.Build();

// Pending migrations are applied on startup; the call does nothing when the schema is already up to date.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ICHU API v1"));
}

app.UseCors("AllowAllPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
