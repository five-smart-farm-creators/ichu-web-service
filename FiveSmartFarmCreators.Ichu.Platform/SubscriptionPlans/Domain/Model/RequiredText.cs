namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;

public static class RequiredText
{
    public static string Normalize(string? value, int maximumLength, string field)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0 || normalized.Length > maximumLength)
        {
            throw SubscriptionException.InvalidInput($"The {field} must contain 1 to {maximumLength} characters.");
        }

        return normalized;
    }
}
