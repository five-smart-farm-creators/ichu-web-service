namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;

public sealed class SubscriptionException(SubscriptionErrorKind kind, string message) : Exception(message)
{
    public SubscriptionErrorKind Kind { get; } = kind;

    public static SubscriptionException InvalidInput(string message) => new(SubscriptionErrorKind.InvalidInput, message);

    public static SubscriptionException Unauthenticated(string message) => new(SubscriptionErrorKind.Unauthenticated, message);

    public static SubscriptionException Forbidden(string message) => new(SubscriptionErrorKind.Forbidden, message);

    public static SubscriptionException NotFound(string message) => new(SubscriptionErrorKind.NotFound, message);

    public static SubscriptionException Conflict(string message) => new(SubscriptionErrorKind.Conflict, message);

    public static SubscriptionException Unavailable(string message) => new(SubscriptionErrorKind.Unavailable, message);
}
