using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public static class MonitoringValueParser
{
    /// <summary>Parses a single enumeration name case-insensitively; numeric values and combined names are rejected.</summary>
    public static TEnum ParseEnum<TEnum>(string? value, string field) where TEnum : struct, Enum
    {
        var text = value?.Trim();
        var match = Enum.GetNames<TEnum>().FirstOrDefault(name => string.Equals(name, text, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            throw MonitoringException.InvalidInput($"The {field} must be one of: {string.Join(", ", Enum.GetNames<TEnum>())}.");
        }

        return Enum.Parse<TEnum>(match);
    }

    public static TEnum? ParseOptionalEnum<TEnum>(string? value, string field) where TEnum : struct, Enum =>
        string.IsNullOrWhiteSpace(value) ? null : ParseEnum<TEnum>(value, field);
}
