using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Microsoft.EntityFrameworkCore;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.DependencyInjection;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var planningConnectionString = builder.Configuration.GetConnectionString("Planning")
    ?? throw new InvalidOperationException(
        "Configure the Planning PostgreSQL connection string using ConnectionStrings:Planning.");
builder.Services.AddPlanning(planningConnectionString);

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

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ICHU API v1");
    });
}

await app.Services.InitializePlanningDatabaseAsync();

app.UseCors("AllowAllPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
