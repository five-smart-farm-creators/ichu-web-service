namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

/// <summary>
///     A telemetry reading that supports an alert.
/// </summary>
public class AlertEvidence
{
    private AlertEvidence()
    {
    }

    public AlertEvidence(Guid deviceId, Guid readingId)
    {
        DeviceId = deviceId;
        ReadingId = readingId;
    }

    public Guid DeviceId { get; private set; }

    public Guid ReadingId { get; private set; }
}
