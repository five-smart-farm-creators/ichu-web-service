using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A metric captured by a device: a finite value, an explicitly invalid capture with its reason, or no capture. Absent and invalid captures never carry a substitute value, so a missing sensor is never read as zero.
/// </summary>
public record Measurement
{
    public const int MaximumReasonLength = 200;

    private Measurement(double? value, string? invalidReason)
    {
        Value = value;
        InvalidReason = invalidReason;
    }

    public double? Value { get; }

    public string? InvalidReason { get; }

    public MeasurementStatus Status => Value is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    public bool IsValid => Value is not null;

    public static Measurement Absent() => new(null, null);

    public static Result<Measurement> From(double? value, string? invalidReason)
    {
        if (value is not null && !string.IsNullOrWhiteSpace(invalidReason))
        {
            return Result<Measurement>.Failure(OperationsMonitoringErrors.MeasurementValidAndInvalid);
        }

        if (value is { } captured)
        {
            return double.IsFinite(captured)
                ? Result<Measurement>.Success(new Measurement(captured, null))
                : Result<Measurement>.Failure(OperationsMonitoringErrors.MeasurementNotFinite);
        }

        if (string.IsNullOrWhiteSpace(invalidReason))
        {
            return Result<Measurement>.Success(Absent());
        }

        var reason = NormalizeReason(invalidReason);
        return reason.IsFailure
            ? Result<Measurement>.Failure(reason.DomainError)
            : Result<Measurement>.Success(new Measurement(null, reason.Value));
    }

    public static Result<string> NormalizeReason(string reason)
    {
        var normalized = reason.Trim();
        return normalized.Length is 0 or > MaximumReasonLength
            ? Result<string>.Failure(OperationsMonitoringErrors.InvalidMeasurementReason)
            : Result<string>.Success(normalized);
    }
}
