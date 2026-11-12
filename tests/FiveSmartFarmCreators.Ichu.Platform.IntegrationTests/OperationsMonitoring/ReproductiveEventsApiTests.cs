using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.OperationsMonitoring;

[Collection(ApiCollection.Name)]
public sealed class ReproductiveEventsApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task RecordReproductiveEvents_ThroughTheEndpointOfTheirKind()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0500");
        var now = DateTimeOffset.UtcNow;

        using var insemination = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", now.AddDays(-200),
            sireReference: "Bull 07");
        using var diagnosis = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", now.AddDays(-180),
            pregnancyResult: "Pregnant", expectedCalvingDate: DateOnly.FromDateTime(now.AddDays(-180).UtcDateTime).AddDays(200));
        using var natural = await RecordAsync(client, ranchId, "field-events", cattleId, "NaturalMating", now.AddDays(-150), sireReference: "Bull 12");
        using var heat = await RecordAsync(client, ranchId, "field-events", cattleId, "HeatObserved", now.AddDays(-120));
        using var abortion = await RecordAsync(client, ranchId, "field-events", cattleId, "Abortion", now.AddDays(-100));
        using var birth = await RecordAsync(client, ranchId, "field-events", cattleId, "Birth", now.AddDays(-10));

        var recordedService = await insemination.ReadJsonAsync(HttpStatusCode.Created);
        Assert.True(recordedService.GetProperty("isClinical").GetBoolean());
        Assert.Equal("Bull 07", recordedService.GetProperty("sireReference").GetString());
        var recordedDiagnosis = await diagnosis.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal("Pregnant", recordedDiagnosis.GetProperty("pregnancyResult").GetString());
        Assert.Equal(DateOnly.FromDateTime(now.AddDays(-180).UtcDateTime).AddDays(200).ToString("yyyy-MM-dd"),
            recordedDiagnosis.GetProperty("expectedCalvingDate").GetString());
        var recordedMating = await natural.ReadJsonAsync(HttpStatusCode.Created);
        Assert.False(recordedMating.GetProperty("isClinical").GetBoolean());
        await heat.ReadBodyAsync(HttpStatusCode.Created);
        await abortion.ReadBodyAsync(HttpStatusCode.Created);
        await birth.ReadBodyAsync(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("field-events", "Insemination", "OperationsMonitoring.FieldEventTypeOnly")]
    [InlineData("field-events", "PregnancyDiagnosis", "OperationsMonitoring.FieldEventTypeOnly")]
    [InlineData("clinical-interventions", "NaturalMating", "OperationsMonitoring.ClinicalInterventionTypeOnly")]
    [InlineData("clinical-interventions", "Birth", "OperationsMonitoring.ClinicalInterventionTypeOnly")]
    public async Task ReproductiveEvent_ThroughTheWrongEndpoint_IsRefused(string route, string type, string code)
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0501");

        using var response = await RecordAsync(client, ranchId, route, cattleId, type, DateTimeOffset.UtcNow.AddHours(-1));

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal(code, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ReproductiveDetails_NotAcceptedByTheirType_AreRejected()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0502");
        var occurredAt = DateTimeOffset.UtcNow.AddDays(-30);

        using var birthWithSire = await RecordAsync(client, ranchId, "field-events", cattleId, "Birth", occurredAt, sireReference: "Bull 07");
        using var diagnosisWithoutResult = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", occurredAt);
        using var inseminationWithResult = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", occurredAt,
            pregnancyResult: "Pregnant");
        using var unknownResult = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", occurredAt,
            pregnancyResult: "Maybe");
        using var openWithCalving = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", occurredAt,
            pregnancyResult: "Open", expectedCalvingDate: DateOnly.FromDateTime(occurredAt.UtcDateTime).AddDays(100));
        using var calvingTooLate = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", occurredAt,
            pregnancyResult: "Pregnant", expectedCalvingDate: DateOnly.FromDateTime(occurredAt.UtcDateTime).AddDays(301));
        using var longSire = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", occurredAt,
            sireReference: new string('s', ReproductiveDetails.MaximumSireReferenceLength + 1));

        Assert.Equal("OperationsMonitoring.ReproductiveDetailsNotAllowed", (await birthWithSire.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.PregnancyResultRequired", (await diagnosisWithoutResult.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.ReproductiveDetailsNotAllowed", (await inseminationWithResult.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.PregnancyResultUnknown", (await unknownResult.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.ExpectedCalvingDateInvalid", (await openWithCalving.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.ExpectedCalvingDateInvalid", (await calvingTooLate.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
        Assert.Equal("OperationsMonitoring.SireReferenceTooLong", (await longSire.ReadJsonAsync(HttpStatusCode.BadRequest)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task ReproductiveEvent_OfAMale_IsRefused()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var bullId = await RegisterAsync(client, ranchId, "PE-0503", sex: 2, lifeStage: 4, birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-6));

        using var response = await RecordAsync(client, ranchId, "clinical-interventions", bullId, "Insemination", DateTimeOffset.UtcNow.AddDays(-5),
            sireReference: "Bull 07");

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.ReproductiveEventRequiresFemale", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RetriedReproductiveEvent_ReturnsTheStoredRecord_AndAChangedSireConflicts()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0504");
        var operationId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow.AddDays(-5);
        using var first = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", occurredAt,
            sireReference: "Bull 07", operationId: operationId);
        await first.ReadBodyAsync(HttpStatusCode.Created);

        using var retried = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", occurredAt,
            sireReference: "Bull 07", operationId: operationId);
        using var changed = await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", occurredAt,
            sireReference: "Bull 08", operationId: operationId);

        await retried.ReadBodyAsync(HttpStatusCode.OK);
        var conflict = await changed.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("OperationsMonitoring.OperationConflict", conflict.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ReproductiveHistory_ForFemaleWithTwoCalvingsAndServices_ReportsTheExactNumbers()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0505");
        var now = DateTimeOffset.UtcNow;
        await RecordAsync(client, ranchId, "field-events", cattleId, "Birth", now.AddDays(-600));
        await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", now.AddDays(-550), sireReference: "Bull 07");
        await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", now.AddDays(-530), pregnancyResult: "Pregnant");
        await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", now.AddDays(-500), pregnancyResult: "Pregnant");
        await RecordAsync(client, ranchId, "field-events", cattleId, "HeatObserved", now.AddDays(-470));
        await RecordAsync(client, ranchId, "field-events", cattleId, "Birth", now.AddDays(-300));
        await RecordAsync(client, ranchId, "field-events", cattleId, "NaturalMating", now.AddDays(-280), sireReference: "Bull 12");
        await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "Insemination", now.AddDays(-230), sireReference: "Bull 07");
        await RecordAsync(client, ranchId, "clinical-interventions", cattleId, "PregnancyDiagnosis", now.AddDays(-200), pregnancyResult: "Pregnant");

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/reproductive-history");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Female", history.GetProperty("sex").GetString());
        Assert.Equal("MilkingCow", history.GetProperty("lifeStage").GetString());
        Assert.True(history.GetProperty("hasHistory").GetBoolean());
        Assert.Equal(3, history.GetProperty("services").GetArrayLength());
        Assert.Equal(3, history.GetProperty("pregnancyDiagnoses").GetArrayLength());
        Assert.Equal(2, history.GetProperty("calvings").GetArrayLength());
        Assert.Equal(1, history.GetProperty("heatsObserved").GetArrayLength());
        Assert.Equal(0, history.GetProperty("abortions").GetArrayLength());
        Assert.Equal(2, history.GetProperty("pregnanciesConfirmed").GetInt32());
        Assert.Equal(1.5, history.GetProperty("servicesPerPregnancy").GetDouble());
        var intervals = history.GetProperty("intervals").EnumerateArray().ToList();
        Assert.Equal(2, intervals.Count);
        Assert.Equal(now.AddDays(-600).Date, intervals[0].GetProperty("calvingAt").GetDateTimeOffset().Date);
        Assert.Equal(MonitoringTime.Normalize(now.AddDays(-550)), intervals[0].GetProperty("conceptionServiceAt").GetDateTimeOffset());
        Assert.Equal(50, intervals[0].GetProperty("daysOpen").GetInt32());
        Assert.Equal(now.AddDays(-300).Date, intervals[1].GetProperty("calvingAt").GetDateTimeOffset().Date);
        Assert.Equal(MonitoringTime.Normalize(now.AddDays(-230)), intervals[1].GetProperty("conceptionServiceAt").GetDateTimeOffset());
        Assert.Equal(70, intervals[1].GetProperty("daysOpen").GetInt32());
    }

    [Fact]
    public async Task ReproductiveHistory_ForAnimalWithoutHistory_ReportsItsAgeAndStage()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cowId = await RegisterAsync(client, ranchId, "PE-0506", sex: 1, lifeStage: 3,
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-10));

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cowId}/reproductive-history");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(10, history.GetProperty("ageInMonths").GetInt32());
        Assert.Equal("MilkingCow", history.GetProperty("lifeStage").GetString());
        Assert.False(history.GetProperty("hasHistory").GetBoolean());
        Assert.Equal(0, history.GetProperty("pregnanciesConfirmed").GetInt32());
        Assert.Equal(JsonValueKind.Null, history.GetProperty("servicesPerPregnancy").ValueKind);
    }

    [Fact]
    public async Task ReproductiveHistory_ForMale_ReportsSexStageAndAgeWithoutHistory()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var bullId = await RegisterAsync(client, ranchId, "PE-0507", sex: 2, lifeStage: 4,
            birthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-6));

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{bullId}/reproductive-history");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Male", history.GetProperty("sex").GetString());
        Assert.Equal("FatteningBull", history.GetProperty("lifeStage").GetString());
        Assert.Equal(6, history.GetProperty("ageInMonths").GetInt32());
        Assert.False(history.GetProperty("hasHistory").GetBoolean());
    }

    [Fact]
    public async Task ReproductiveHistory_ByAccountOutsideTheRanch_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(owner, ranchId, "PE-0508");
        using var other = factory.CreateClient(TestIdentity.Create());
        await other.RegisterManagerAsync();

        using var response = await other.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/reproductive-history");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("OperationsMonitoring.Forbidden", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ReproductiveHistory_ForUnknownAnimal_IsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{Guid.NewGuid()}/reproductive-history");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.AnimalNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MobileSync_WithThePayloadOfTheCurrentClients_IsAccepted()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0509");
        var occurredAt = DateTimeOffset.UtcNow.AddHours(-2);

        using var response = await client.PostJsonAsync("api/v1/mobile-sync/batch", new
        {
            operations = new object[]
            {
                new { operationId = Guid.NewGuid(), cattleId, type = "HealthEvent", occurredAt, observation = "Mild lameness" },
                new { operationId = Guid.NewGuid(), cattleId, type = "Treatment", occurredAt, product = "Antibiotic", dose = "5 ml" }
            }
        });

        var results = (await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray().ToList();
        Assert.All(results, entry => Assert.Equal("Accepted", entry.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task MobileSync_WithReproductiveEvents_AcceptsTheirFieldsAndRejectsAMale()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = await RegisterFemaleAsync(client, ranchId, "PE-0510");
        var bullId = await RegisterAsync(client, ranchId, "PE-0511", sex: 2, lifeStage: 4, birthDate: new DateOnly(2025, 1, 1));
        var occurredAt = DateTimeOffset.UtcNow.AddDays(-3);

        using var response = await client.PostJsonAsync("api/v1/mobile-sync/batch", new
        {
            operations = new object[]
            {
                new { operationId = Guid.NewGuid(), cattleId, type = "Insemination", occurredAt, sireReference = "Bull 07" },
                new { operationId = Guid.NewGuid(), cattleId, type = "PregnancyDiagnosis", occurredAt, pregnancyResult = "Pregnant" },
                new { operationId = Guid.NewGuid(), cattleId, type = "Birth", occurredAt },
                new { operationId = Guid.NewGuid(), cattleId = bullId, type = "NaturalMating", occurredAt, sireReference = "Bull 12" }
            }
        });

        var statuses = (await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray()
            .Select(entry => entry.GetProperty("status").GetString()).ToList();
        Assert.Equal(new[] { "Accepted", "Accepted", "Accepted", "Rejected" }, statuses);
    }

    private static async Task<Guid> RegisterFemaleAsync(HttpClient client, Guid ranchId, string earTag)
    {
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, earTag, lifeStage: 3);
        return cattle.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> RegisterAsync(HttpClient client, Guid ranchId, string earTag, int sex, int lifeStage, DateOnly birthDate)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/cattle", new
        {
            earTag,
            breed = 1,
            sex,
            birthDate,
            lifeStage
        });
        var cattle = await response.ReadJsonAsync(HttpStatusCode.Created);
        return cattle.GetProperty("id").GetGuid();
    }

    private static Task<HttpResponseMessage> RecordAsync(
        HttpClient client,
        Guid ranchId,
        string route,
        Guid cattleId,
        string type,
        DateTimeOffset occurredAt,
        string? sireReference = null,
        string? pregnancyResult = null,
        DateOnly? expectedCalvingDate = null,
        Guid? operationId = null) =>
        client.PostJsonAsync($"api/v1/ranches/{ranchId}/{route}", new
        {
            operationId = operationId ?? Guid.NewGuid(),
            cattleId,
            type,
            occurredAt = MonitoringTime.Normalize(occurredAt),
            sireReference,
            pregnancyResult,
            expectedCalvingDate
        });
}
