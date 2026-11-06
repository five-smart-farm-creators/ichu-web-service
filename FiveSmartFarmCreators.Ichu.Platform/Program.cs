using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.DependencyInjection;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var planningConnectionString = builder.Configuration.GetConnectionString("Planning")
    ?? throw new InvalidOperationException(
        "Configure the Planning PostgreSQL connection string using ConnectionStrings:Planning.");
builder.Services.AddPlanning(planningConnectionString);

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
