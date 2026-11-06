namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a metric captured by a device: a finite value, an explicitly invalid capture with
///     its reason, or no capture at all. Absent and invalid captures never carry a substitute value.
/// </summary>
public sealed record Measurement
{
    /// <summary>Maximum number of characters of an invalid-capture reason.</summary>
    public const int MaximumReasonLength = 200;

    private Measurement(double? value, string? invalidReason)
    {
        Value = value;
        InvalidReason = invalidReason;
    }

    /// <summary>Gets the captured value, or <see langword="null"/> when the capture is invalid or absent.</summary>
    public double? Value { get; }

    /// <summary>Gets the reason the device marked the capture as invalid, if it did.</summary>
    public string? InvalidReason { get; }

    /// <summary>Gets the quality of the capture.</summary>
    public MeasurementStatus Status => Value is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    /// <summary>Gets a value indicating whether the capture carries a value.</summary>
    public bool IsValid => Value is not null;

    /// <summary>
    ///     Creates a measurement for a metric the device did not send.
    /// </summary>
    /// <returns>An absent measurement.</returns>
    public static Measurement Absent() => new(null, null);

    /// <summary>
    ///     Creates a measurement from the data sent by a device.
    /// </summary>
    /// <param name="value">The captured value, if any.</param>
    /// <param name="invalidReason">The reason the capture is invalid, if the device reported one.</param>
    /// <returns>A valid, invalid or absent measurement.</returns>
    /// <exception cref="ArgumentException">Thrown when the capture is both valid and invalid, or the value is not finite.</exception>
    public static Measurement From(double? value, string? invalidReason)
    {
        if (value is not null && !string.IsNullOrWhiteSpace(invalidReason))
        {
            throw new ArgumentException("A measurement cannot be valid and invalid at the same time.");
        }

        if (value is { } captured)
        {
            if (!double.IsFinite(captured))
            {
                throw new ArgumentException("A measurement value must be a finite number.");
            }

            return new Measurement(captured, null);
        }

        return string.IsNullOrWhiteSpace(invalidReason) ? Absent() : new Measurement(null, NormalizeReason(invalidReason));
    }

    /// <summary>
    ///     Trims an invalid-capture reason and checks its length.
    /// </summary>
    /// <param name="reason">The reason sent by the device.</param>
    /// <returns>The trimmed reason.</returns>
    /// <exception cref="ArgumentException">Thrown when the reason is empty or too long.</exception>
    public static string NormalizeReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length == 0 || normalized.Length > MaximumReasonLength)
        {
            throw new ArgumentException($"An invalid-measurement reason must contain 1 to {MaximumReasonLength} characters.");
        }

        return normalized;
    }
}
