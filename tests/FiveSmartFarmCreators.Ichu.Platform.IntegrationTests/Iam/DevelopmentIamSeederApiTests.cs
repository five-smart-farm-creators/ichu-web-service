using System.Net;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Iam;

[Collection(ApiCollection.Name)]
public sealed class DevelopmentIamSeederApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task SeedAsync_RunTwiceForVerifiedManager_GivesManageAccessToTheSeededRanch()
    {
        var identity = TestIdentity.Create();
        await SeedAsync(identity.Uid, identity.Email);
        await SeedAsync(identity.Uid, identity.Email);

        using var client = factory.CreateClient(identity);
        using var response = await client.GetAsync("api/v1/accounts/me/ranches");

        var ranches = await response.ReadJsonAsync(HttpStatusCode.OK);
        var ranch = Assert.Single(ranches.EnumerateArray(),
            access => access.GetProperty("ranchId").GetGuid() == DevelopmentSeed.RanchId);
        Assert.Equal("Manage", ranch.GetProperty("scope").GetString());
        Assert.Equal(DevelopmentSeed.RanchAccessCode, ranch.GetProperty("accessCode").GetString());
    }

    [Fact]
    public async Task SeedAsync_WithoutIdentitySettings_CreatesNothing()
    {
        var before = await CountIamRowsAsync();

        await SeedAsync(externalUid: null, email: null);

        Assert.Equal(before, await CountIamRowsAsync());
    }

    private async Task SeedAsync(string? externalUid, string? email)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DevelopmentSeed:Manager:ExternalUid"] = externalUid,
                ["DevelopmentSeed:Manager:Email"] = email
            })
            .Build();
        using var scope = factory.Services.CreateScope();
        var seeder = ActivatorUtilities.CreateInstance<DevelopmentIamSeeder>(scope.ServiceProvider, configuration);
        await seeder.SeedAsync(CancellationToken.None);
    }

    private async Task<(int Accounts, int Ranches, int Memberships)> CountIamRowsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (
            await context.Set<UserAccount>().CountAsync(),
            await context.Set<RanchRegistration>().CountAsync(),
            await context.Set<RanchMembership>().CountAsync());
    }
}
