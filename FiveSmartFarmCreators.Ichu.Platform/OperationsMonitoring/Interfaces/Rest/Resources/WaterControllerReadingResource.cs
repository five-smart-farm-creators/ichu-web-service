using System.Text.Json;
using System.Text.Json.Serialization;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record WaterControllerReadingResource(
    [property: JsonRequired] Guid ReadingId,
    [property: JsonRequired] decimal Temperature,
    [property: JsonRequired] WaterTemperatureUnit TemperatureUnit,
    [property: JsonRequired] bool IsValid,
    [property: JsonRequired] WaterHeaterState HeaterState,
    [property: JsonRequired, JsonConverter(typeof(OffsetDateTimeJsonConverter))] DateTimeOffset CapturedAt,
    JsonElement? Events = null);
