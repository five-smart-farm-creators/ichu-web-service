using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed class OffsetDateTimeJsonConverter : JsonConverter<DateTimeOffset>
{
    private static readonly Regex ExplicitOffset = new(@"(?:Z|[+-]\d{2}:\d{2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("The timestamp must be an ISO 8601 string with an explicit UTC offset.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value) || !ExplicitOffset.IsMatch(value)
            || !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var timestamp))
        {
            throw new JsonException("The timestamp must be ISO 8601 and include an explicit UTC offset.");
        }

        return timestamp.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToUniversalTime());
}
