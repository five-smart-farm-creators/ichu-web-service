using System.Globalization;
using System.Text.Json;
using Reqnroll;
using Xunit;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Compares response fields with the field and value rows of a Gherkin table, resolving aliases to the identifiers and times of the scenario.
/// </summary>
public static class TableAssertions
{
    private const string NullValue = "null";
    private const string NotNullValue = "not null";
    private const string TimeOfPrefix = "<time of ";
    private const double Tolerance = 1e-9;

    /// <summary>
    ///     Asserts each row of a table with field and value columns, where a field is a dotted path into the element.
    /// </summary>
    public static void AssertFields(ApiScenarioContext context, JsonElement element, DataTable table)
    {
        foreach (var row in table.Rows)
        {
            var field = row["field"];
            var actual = Navigate(element, field);
            Assert.True(Matches(context, actual, row["value"]),
                $"Field '{field}' is {Describe(actual)}, but the scenario expects '{row["value"]}'.");
        }
    }

    /// <summary>
    ///     Returns the item of an array whose text property equals the value, such as the item that carries the identifier of an alias.
    /// </summary>
    public static JsonElement FindByText(JsonElement array, string property, string value)
    {
        var match = array.EnumerateArray().FirstOrDefault(item =>
            item.TryGetProperty(property, out var found) && found.ValueKind == JsonValueKind.String && found.GetString() == value);
        Assert.True(match.ValueKind != JsonValueKind.Undefined, $"No item has {property} '{value}'.");
        return match;
    }

    private static bool Matches(ApiScenarioContext context, JsonElement actual, string expected)
    {
        if (expected == NullValue)
        {
            return IsNull(actual);
        }

        if (expected == NotNullValue)
        {
            return !IsNull(actual);
        }

        // A list field matches when every comma-separated expected value is one of its items.
        if (actual.ValueKind == JsonValueKind.Array)
        {
            return expected.Split(',').Select(token => token.Trim())
                .All(token => actual.EnumerateArray().Any(item => Matches(context, item, token)));
        }

        if (expected.StartsWith(TimeOfPrefix, StringComparison.Ordinal))
        {
            var instant = context.Time(expected[TimeOfPrefix.Length..^1]);
            return actual.ValueKind == JsonValueKind.String &&
                   DateTimeOffset.Parse(actual.GetString()!, CultureInfo.InvariantCulture) == instant;
        }

        if (IsAlias(expected))
        {
            var id = context.Id(expected[1..^1]);
            return actual.ValueKind == JsonValueKind.String && Guid.TryParse(actual.GetString(), out var parsed) && parsed == id;
        }

        return actual.ValueKind switch
        {
            JsonValueKind.Number => double.TryParse(expected, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                                    && Math.Abs(actual.GetDouble() - number) < Tolerance,
            JsonValueKind.True or JsonValueKind.False => bool.TryParse(expected, out var flag) && actual.GetBoolean() == flag,
            JsonValueKind.String => actual.GetString() == expected,
            _ => false
        };
    }

    private static JsonElement Navigate(JsonElement element, string path)
    {
        var current = element;
        foreach (var segment in path.Split('.'))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return default;
            }

            current = next;
        }

        return current;
    }

    private static bool IsAlias(string expected) => expected.Length > 2 && expected[0] == '<' && expected[^1] == '>';

    private static bool IsNull(JsonElement element) => element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;

    private static string Describe(JsonElement element) => element.ValueKind == JsonValueKind.Undefined ? "missing" : element.ToString();
}
