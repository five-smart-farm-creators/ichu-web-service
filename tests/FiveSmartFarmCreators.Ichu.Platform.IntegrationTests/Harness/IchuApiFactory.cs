using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Hosts the API in the Testing environment against a PostgreSQL database created for this test run.
/// </summary>
public sealed class IchuApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionVariable = "ICHU_TEST_CONNECTION";

    // Program creates and migrates this database on startup and expands the password placeholder itself.
    private readonly string _applicationConnectionString;

    public IchuApiFactory()
    {
        var template = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new InvalidOperationException(
                $"The {ConnectionVariable} environment variable must contain the PostgreSQL connection string without a database.");
        }

        var databaseName = $"ichu_it_{Guid.NewGuid():N}"[..16];
        _applicationConnectionString = $"{template.TrimEnd(';')};Database={databaseName}";
    }

    public FakeIdentityProvider Identity { get; } = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await DropDatabaseAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = _applicationConnectionString }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IIdentityProvider>();
            services.AddSingleton<IIdentityProvider>(Identity);
        });
    }

    private async Task DropDatabaseAsync()
    {
        var connectionString = Environment.ExpandEnvironmentVariables(_applicationConnectionString);
        // Idle pooled connections keep the database busy, which makes DROP DATABASE fail.
        NpgsqlConnection.ClearPool(new NpgsqlConnection(connectionString));
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;
        await using var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
    }
}
