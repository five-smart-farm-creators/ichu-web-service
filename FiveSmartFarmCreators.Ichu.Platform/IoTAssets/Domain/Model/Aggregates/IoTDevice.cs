using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

public partial class IoTDevice : AggregateRoot
{
    private readonly List<DeviceAssignment> _assignments = [];

    private IoTDevice()
    {
    }

    public IoTDevice(RegisterDeviceCommand command, DateTimeOffset registeredAt)
    {
        if (command.RanchId == Guid.Empty || !Enum.IsDefined(command.Type) || registeredAt == default
            || command.FirmwareVersion?.Trim().Length > 20)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        Id = Guid.NewGuid();
        RanchId = command.RanchId;
        Serial = new DeviceSerial(command.Serial);
        Type = command.Type;
        FirmwareVersion = string.IsNullOrWhiteSpace(command.FirmwareVersion) ? null : command.FirmwareVersion.Trim();
        Status = DeviceStatus.Available;
        Version = Guid.NewGuid();
        CreatedAt = registeredAt.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public DeviceSerial Serial { get; private set; } = null!;
    public DeviceType Type { get; private set; }
    public DeviceStatus Status { get; private set; }
    public string? FirmwareVersion { get; private set; }
    public DateTimeOffset? LastCommunicationAt { get; private set; }
    public DateTimeOffset? LastCommunicationReceivedAt { get; private set; }
    public BatteryLevel? Battery { get; private set; }
    public DateTimeOffset? BatteryRecordedAt { get; private set; }
    public Guid Version { get; private set; }
    public IReadOnlyCollection<DeviceAssignment> Assignments => _assignments.AsReadOnly();

    public bool IsAvailable() => Status == DeviceStatus.Available && _assignments.All(a => !a.IsOpen());

    public CommunicationStatus GetCommunicationStatus(DateTimeOffset now)
    {
        if (LastCommunicationAt is null)
        {
            return CommunicationStatus.NeverReported;
        }

        var timeout = Type == DeviceType.SmartCollar ? TimeSpan.FromHours(48) : TimeSpan.FromHours(6);
        return now - LastCommunicationAt.Value > timeout
            ? CommunicationStatus.NoCommunication
            : CommunicationStatus.Communicating;
    }

    public DeviceAssignment AssignTo(Guid targetId, DateTimeOffset at)
    {
        if (!IsAvailable() || _assignments.Any(a => a.EndedAt > at))
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidState);
        }

        var targetType = Type == DeviceType.SmartCollar ? AssignmentTargetType.Cattle : AssignmentTargetType.WaterTrough;
        var assignment = new DeviceAssignment(Id, RanchId, targetId, targetType, at);
        _assignments.Add(assignment);
        Status = DeviceStatus.InService;
        Touch(at);
        AddDomainEvent(new DeviceAssignedEvent(Guid.NewGuid(), at.ToUniversalTime(), RanchId, Id,
            assignment.Id, targetId, targetType));
        return assignment;
    }

    public DeviceAssignment Release(string reason, DateTimeOffset at)
    {
        var assignment = _assignments.SingleOrDefault(a => a.IsOpen());
        if (assignment is null)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidState);
        }

        assignment.Close(reason, at);
        Status = DeviceStatus.Available;
        Touch(at);
        AddDomainEvent(new DeviceReleasedEvent(Guid.NewGuid(), at.ToUniversalTime(), RanchId, Id,
            assignment.Id, assignment.TargetId, assignment.TargetType, assignment.ReleaseReason!));
        return assignment;
    }

    public void SetMaintenance(bool inMaintenance, DateTimeOffset at)
    {
        if (_assignments.Any(a => a.IsOpen()) || at == default)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidState);
        }

        var status = inMaintenance ? DeviceStatus.Maintenance : DeviceStatus.Available;
        if (Status != status)
        {
            Status = status;
            Touch(at);
        }
    }

    public void RegisterCommunication(DateTimeOffset communicatedAt, DateTimeOffset receivedAt,
        BatteryLevel? battery, DateTimeOffset now)
    {
        if (communicatedAt == default || receivedAt == default || communicatedAt > receivedAt || receivedAt > now)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        if (battery is not null && BatteryRecordedAt == communicatedAt && Battery != battery)
        {
            throw new IoTAssetsException(IoTAssetsErrors.Conflict);
        }

        var changed = false;
        if (LastCommunicationAt is null || communicatedAt > LastCommunicationAt)
        {
            LastCommunicationAt = communicatedAt.ToUniversalTime();
            changed = true;
        }

        if (LastCommunicationReceivedAt is null || receivedAt > LastCommunicationReceivedAt)
        {
            LastCommunicationReceivedAt = receivedAt.ToUniversalTime();
            changed = true;
        }

        if (battery is not null && (BatteryRecordedAt is null || communicatedAt > BatteryRecordedAt))
        {
            Battery = battery;
            BatteryRecordedAt = communicatedAt.ToUniversalTime();
            changed = true;
        }

        if (changed)
        {
            Touch(now);
        }
    }

    public void EnsureVersion(Guid expectedVersion)
    {
        if (expectedVersion == Guid.Empty)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        if (Version != expectedVersion)
        {
            throw new IoTAssetsException(IoTAssetsErrors.Conflict);
        }
    }

    private void Touch(DateTimeOffset at)
    {
        Version = Guid.NewGuid();
        UpdatedAt = at.ToUniversalTime();
    }
}
