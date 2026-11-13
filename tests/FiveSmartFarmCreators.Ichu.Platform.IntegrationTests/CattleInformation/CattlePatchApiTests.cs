using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.CattleInformation;

[Collection(ApiCollection.Name)]
public sealed class CattlePatchApiTests(IchuApiFactory factory)
{
    [Theory]
    [InlineData("earTag", "PE-CHANGED")]
    [InlineData("breed", "Fleckvieh")]
    [InlineData("sex", "Male")]
    [InlineData("birthDate", "2023-12-01")]
    public async Task Patch_WithOneIdentityField_ChangesOnlyThatFieldAndVersion(string field, string value)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-PARTIAL");
        var cattleId = original.GetProperty("id").GetGuid();
        var patch = new Dictionary<string, object>
        {
            ["expectedVersion"] = original.GetProperty("version").GetGuid(),
            [field] = value
        };

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), patch);

        var updated = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(value, updated.GetProperty(field).GetString());
        Assert.NotEqual(original.GetProperty("version").GetGuid(), updated.GetProperty("version").GetGuid());
        foreach (var property in original.EnumerateObject().Where(property => property.Name != field && property.Name != "version"))
        {
            Assert.Equal(property.Value.GetRawText(), updated.GetProperty(property.Name).GetRawText());
        }

        Assert.Equal(updated.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task Patch_LifeStage_RetainsThePreviousPeriodAndPersistsTheNewOne()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-STAGE");
        var cattleId = original.GetProperty("id").GetGuid();
        var validFrom = DateOnly.FromDateTime(DateTime.UtcNow);

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            lifeStage = "MilkingCow",
            validFrom,
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var updated = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("MilkingCow", updated.GetProperty("currentStage").GetString());
        Assert.Equal(original.GetProperty("earTag").GetString(), updated.GetProperty("earTag").GetString());
        var periods = updated.GetProperty("stagePeriods").EnumerateArray().ToList();
        Assert.Equal(2, periods.Count);
        var previous = Assert.Single(periods, period => period.GetProperty("lifeStage").GetString() == "Heifer");
        Assert.Equal(original.GetProperty("stagePeriods")[0].GetProperty("id").GetGuid(), previous.GetProperty("id").GetGuid());
        Assert.Equal(validFrom.ToString("yyyy-MM-dd"), previous.GetProperty("validTo").GetString());
        var current = Assert.Single(periods, period => period.GetProperty("lifeStage").GetString() == "MilkingCow");
        Assert.Equal(validFrom.ToString("yyyy-MM-dd"), current.GetProperty("validFrom").GetString());
        Assert.Equal(JsonValueKind.Null, current.GetProperty("validTo").ValueKind);
        Assert.Equal(updated.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task Patch_CombinedIdentityAndStage_PersistsAllChangesAtTheExpectedVersion()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-COMBINED");
        var cattleId = original.GetProperty("id").GetGuid();

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-COMBINED-UPDATED",
            breed = "Fleckvieh",
            sex = "Female",
            birthDate = new DateOnly(2023, 12, 1),
            lifeStage = "MilkingCow",
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var updated = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("PE-COMBINED-UPDATED", updated.GetProperty("earTag").GetString());
        Assert.Equal("Fleckvieh", updated.GetProperty("breed").GetString());
        Assert.Equal("2023-12-01", updated.GetProperty("birthDate").GetString());
        Assert.Equal("MilkingCow", updated.GetProperty("currentStage").GetString());
        Assert.Equal(2, updated.GetProperty("stagePeriods").GetArrayLength());
        Assert.NotEqual(original.GetProperty("version").GetGuid(), updated.GetProperty("version").GetGuid());
        Assert.Equal(updated.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task Patch_WithAStaleVersion_LeavesIdentityAndStageHistoryUnchanged()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-STALE");
        var cattleId = original.GetProperty("id").GetGuid();
        using var first = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-CURRENT",
            expectedVersion = original.GetProperty("version").GetGuid()
        });
        var latest = await first.ReadJsonAsync(HttpStatusCode.OK);

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-STALE-UPDATE",
            lifeStage = "MilkingCow",
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("CattleInformation.CattleVersionMismatch", problem.GetProperty("code").GetString());
        Assert.Equal(latest.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Theory]
    [InlineData(2, 0, HttpStatusCode.Conflict, "CattleInformation.LifeStageAlreadyCurrent")]
    [InlineData(3, -1, HttpStatusCode.BadRequest, "CattleInformation.InvalidLifeStageDate")]
    [InlineData(3, 1, HttpStatusCode.BadRequest, "CattleInformation.InvalidLifeStageDate")]
    [InlineData(999, 0, HttpStatusCode.BadRequest, "CattleInformation.InvalidLifeStage")]
    public async Task Patch_WithAnInvalidStageTransition_DoesNotSaveOtherValidChanges(int lifeStage, int dayOffset,
        HttpStatusCode status, string errorCode)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-INVALID-STAGE");
        var cattleId = original.GetProperty("id").GetGuid();

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-MUST-NOT-CHANGE",
            breed = "Fleckvieh",
            lifeStage,
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(dayOffset),
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var problem = await response.ReadJsonAsync(status);
        Assert.Equal(errorCode, problem.GetProperty("code").GetString());
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Theory]
    [InlineData("breed", "0", "CattleInformation.InvalidBreed")]
    [InlineData("sex", "0", "CattleInformation.InvalidSex")]
    [InlineData("birthDate", "2040-01-01", "CattleInformation.InvalidBirthDate")]
    public async Task Patch_WithInvalidIdentity_DoesNotSaveTheOtherwiseValidStageChange(string field, string value, string errorCode)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-INVALID-IDENTITY");
        var cattleId = original.GetProperty("id").GetGuid();
        var patch = new Dictionary<string, object>
        {
            ["expectedVersion"] = original.GetProperty("version").GetGuid(),
            ["earTag"] = "PE-MUST-NOT-CHANGE",
            ["lifeStage"] = "MilkingCow",
            ["validFrom"] = DateOnly.FromDateTime(DateTime.UtcNow),
            [field] = field == "birthDate" ? value : 0
        };

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), patch);

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal(errorCode, problem.GetProperty("code").GetString());
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Theory]
    [InlineData("missing version")]
    [InlineData("no changes")]
    [InlineData("stage without date")]
    [InlineData("date without stage")]
    public async Task Patch_WithAnIncompleteRequest_IsBadRequestAndSavesNothing(string scenario)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-INCOMPLETE");
        var cattleId = original.GetProperty("id").GetGuid();
        var patch = new Dictionary<string, object>();
        if (scenario != "missing version")
        {
            patch["expectedVersion"] = original.GetProperty("version").GetGuid();
        }

        if (scenario != "no changes")
        {
            patch["earTag"] = "PE-MUST-NOT-CHANGE";
        }

        if (scenario == "stage without date")
        {
            patch["lifeStage"] = "MilkingCow";
        }

        if (scenario == "date without stage")
        {
            patch["validFrom"] = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), patch);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Theory]
    [InlineData("ranchId")]
    [InlineData("id")]
    [InlineData("status")]
    [InlineData("currentStage")]
    [InlineData("deviceId")]
    [InlineData("stagePeriods")]
    [InlineData("version")]
    public async Task Patch_WithAFieldOutsideTheAllowlist_IsBadRequestAndSavesNothing(string field)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-ALLOWLIST");
        var cattleId = original.GetProperty("id").GetGuid();
        var patch = new Dictionary<string, object>
        {
            ["expectedVersion"] = original.GetProperty("version").GetGuid(),
            ["earTag"] = "PE-MUST-NOT-CHANGE",
            [field] = "unsupported"
        };

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), patch);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task Patch_WithADuplicateEarTag_DoesNotSaveTheStageChange()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var holder = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-RESERVED");
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-DUPLICATE");
        var cattleId = original.GetProperty("id").GetGuid();

        using var response = await owner.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-RESERVED",
            lifeStage = "MilkingCow",
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("CattleInformation.DuplicateEarTag", problem.GetProperty("code").GetString());
        Assert.Equal(holder.GetProperty("id").GetGuid(), problem.GetProperty("conflictingCattleId").GetGuid());
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task Patch_ByTheManagerOfAnotherRanch_IsForbiddenEvenWithTheIdAndCurrentVersion()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-PERMISSIONS");
        var cattleId = original.GetProperty("id").GetGuid();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PatchJsonAsync(PatchPath(cattleId), new
        {
            earTag = "PE-MUST-NOT-CHANGE",
            lifeStage = "MilkingCow",
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = original.GetProperty("version").GetGuid()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("CattleInformation.RanchAccessDenied", problem.GetProperty("code").GetString());
        Assert.Equal(original.GetRawText(), (await ReadCattleAsync(owner, ranchId, cattleId)).GetRawText());
    }

    [Fact]
    public async Task ConcurrentPatches_WithTheSameVersion_SaveOnlyOneCompleteChange()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var original = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-CONCURRENT");
        var cattleId = original.GetProperty("id").GetGuid();
        var expectedVersion = original.GetProperty("version").GetGuid();
        var validFrom = DateOnly.FromDateTime(DateTime.UtcNow);

        var responses = await Task.WhenAll(
            owner.PatchJsonAsync(PatchPath(cattleId), new { earTag = "PE-WINNER-A", lifeStage = "MilkingCow", validFrom, expectedVersion }),
            owner.PatchJsonAsync(PatchPath(cattleId), new { earTag = "PE-WINNER-B", lifeStage = "Calf", validFrom, expectedVersion }));

        try
        {
            var success = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
            Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
            var winner = await success.ReadJsonAsync(HttpStatusCode.OK);
            var stored = await ReadCattleAsync(owner, ranchId, cattleId);
            Assert.Equal(2, stored.GetProperty("stagePeriods").GetArrayLength());
            Assert.Equal(winner.GetRawText(), stored.GetRawText());
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Fact]
    public async Task Patch_ForAnUnknownAnimal_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        await owner.RegisterManagerAsync();

        using var response = await owner.PatchJsonAsync(PatchPath(Guid.NewGuid()), new { earTag = "PE-UNKNOWN", expectedVersion = Guid.NewGuid() });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("CattleInformation.CattleNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Patch_WithoutAuthentication_IsUnauthorized()
    {
        using var anonymous = factory.CreateClient();

        using var response = await anonymous.PatchJsonAsync(PatchPath(Guid.NewGuid()), new { earTag = "PE-UNKNOWN", expectedVersion = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string PatchPath(Guid cattleId) => $"api/v1/cattle/{cattleId}";

    private static async Task<JsonElement> ReadCattleAsync(HttpClient owner, Guid ranchId, Guid cattleId)
    {
        using var response = await owner.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}");
        return (await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("cattle");
    }
}
