namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
/// A metric captured by a device: a finite value, an explicitly invalid capture with its reason, or no capture at all.
/// Absent and invalid captures never carry a substitute value.
/// </summary>
public sealed record Measurement
{
    public const int MaximumReasonLength = 200;

    public double? Value { get; }
    public string? InvalidReason { get; }

    private Measurement(double? value, string? invalidReason)
    {
        Value = value;
        InvalidReason = invalidReason;
    }

    public MeasurementStatus Status => Value is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    public bool IsValid => Value is not null;

    public static Measurement Absent() => new(null, null);

    public static Measurement From(double? value, string? invalidReason)
    {
        if (value is not null && !string.IsNullOrWhiteSpace(invalidReason))
        {
            throw MonitoringException.InvalidInput("A measurement cannot be valid and invalid at the same time.");
        }

        if (value is { } captured)
        {
            if (!double.IsFinite(captured))
            {
                throw MonitoringException.InvalidInput("A measurement value must be a finite number.");
            }

            return new Measurement(captured, null);
        }

        return string.IsNullOrWhiteSpace(invalidReason) ? Absent() : new Measurement(null, NormalizeReason(invalidReason));
    }

    public static string NormalizeReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length == 0 || normalized.Length > MaximumReasonLength)
        {
            throw MonitoringException.InvalidInput($"An invalid-measurement reason must contain 1 to {MaximumReasonLength} characters.");
        }

        return normalized;
    }
}
