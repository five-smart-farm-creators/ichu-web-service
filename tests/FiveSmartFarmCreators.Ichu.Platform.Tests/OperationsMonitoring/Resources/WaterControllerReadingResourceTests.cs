using System.Text.Json;
using System.Text.Json.Serialization;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Resources;

public sealed class WaterControllerReadingResourceTests
{
    [Fact]
    public void Deserialize_RequiresExplicitTimestampOffset()
    {
        const string json = """
            {
              "readingId": "d071f398-9d74-4f61-8b4a-70d66362539a",
              "temperature": 16.5,
              "temperatureUnit": "Celsius",
              "isValid": true,
              "heaterState": "On",
              "capturedAt": "2026-11-14T12:00:00.123456"
            }
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WaterControllerReadingResource>(json,
            JsonOptions()));
    }

    [Fact]
    public void Deserialize_RequiresTheValidityFieldInsteadOfDefaultingItToFalse()
    {
        const string json = """
            {
              "readingId": "d071f398-9d74-4f61-8b4a-70d66362539a",
              "temperature": 16.5,
              "temperatureUnit": "Celsius",
              "heaterState": "On",
              "capturedAt": "2026-11-14T12:00:00.123456Z"
            }
            """;

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WaterControllerReadingResource>(json,
            JsonOptions()));
    }

    [Fact]
    public void Deserialize_NormalizesOffsetTimestampToUtc()
    {
        const string json = """
            {
              "readingId": "d071f398-9d74-4f61-8b4a-70d66362539a",
              "temperature": 16.5,
              "temperatureUnit": "Celsius",
              "isValid": true,
              "heaterState": "On",
              "capturedAt": "2026-11-14T15:00:00.123456+03:00"
            }
            """;

        var resource = JsonSerializer.Deserialize<WaterControllerReadingResource>(json, JsonOptions());

        Assert.NotNull(resource);
        Assert.Equal(new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero).AddTicks(1234560), resource.CapturedAt);
        Assert.Equal(WaterTemperatureUnit.Celsius, resource.TemperatureUnit);
        Assert.Equal(WaterHeaterState.On, resource.HeaterState);
    }

    private static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
