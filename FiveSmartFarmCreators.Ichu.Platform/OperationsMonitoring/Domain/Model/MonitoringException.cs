namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

public sealed class MonitoringException(MonitoringErrorKind kind, string message) : Exception(message)
{
    public MonitoringErrorKind Kind { get; } = kind;

    public static MonitoringException InvalidInput(string message) => new(MonitoringErrorKind.InvalidInput, message);

    public static MonitoringException Unauthenticated(string message) => new(MonitoringErrorKind.Unauthenticated, message);

    public static MonitoringException Forbidden(string message) => new(MonitoringErrorKind.Forbidden, message);

    public static MonitoringException NotFound(string message) => new(MonitoringErrorKind.NotFound, message);

    public static MonitoringException Conflict(string message) => new(MonitoringErrorKind.Conflict, message);
}
