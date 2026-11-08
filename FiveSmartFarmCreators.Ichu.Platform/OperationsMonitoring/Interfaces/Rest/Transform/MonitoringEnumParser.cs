namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Matches a name sent by a client to an enum member, ignoring case and surrounding spaces.
/// </summary>
public static class MonitoringEnumParser
{
    public static TEnum? Parse<TEnum>(string? name) where TEnum : struct, Enum
    {
        var trimmed = name?.Trim();
        return Enum.GetNames<TEnum>().FirstOrDefault(candidate => candidate.Equals(trimmed, StringComparison.OrdinalIgnoreCase)) is { } match
            ? Enum.Parse<TEnum>(match)
            : null;
    }
}
