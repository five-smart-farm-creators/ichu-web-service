using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

/// <summary>
///     A collar or water controller of a ranch. It is assigned to one target at a time, and every assignment is kept as history.
/// </summary>
public partial class IoTDevice : AggregateRoot
{
    private readonly List<DeviceAssignment> _assignments = [];

    // Entity Framework Core materializes devices without running the factory.
    private IoTDevice()
    {
    }

    public DeviceId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public DeviceSerial Serial { get; private set; } = null!;

    public DeviceType Type { get; private set; }

    public DeviceStatus Status { get; private set; }

    public string? FirmwareVersion { get; private set; }

    public DateTimeOffset? LastCommunicationAt { get; private set; }

    public DateTimeOffset? LastCommunicationReceivedAt { get; private set; }

    public BatteryLevel? Battery { get; private set; }

    public DateTimeOffset? BatteryRecordedAt { get; private set; }

    /// <summary>
    ///     Advanced by every change, so that a stale caller is refused instead of overwriting newer state.
    /// </summary>
    public Guid Version { get; private set; }

    public IReadOnlyCollection<DeviceAssignment> Assignments => _assignments.AsReadOnly();

    public static Result<IoTDevice> Register(RegisterDeviceCommand command, DateTimeOffset registeredAt)
    {
        if (command.RanchId == Guid.Empty || !Enum.IsDefined(command.Type) || registeredAt == default
            || command.FirmwareVersion?.Trim().Length > 20)
        {
            return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var serial = DeviceSerial.Create(command.Serial);
        if (serial.IsFailure)
        {
            return Result<IoTDevice>.Failure(serial.DomainError);
        }

        var device = new IoTDevice
        {
            Id = new DeviceId(Guid.NewGuid()),
            RanchId = command.RanchId,
            Serial = serial.Value!,
            Type = command.Type,
            FirmwareVersion = string.IsNullOrWhiteSpace(command.FirmwareVersion) ? null : command.FirmwareVersion.Trim(),
            Status = DeviceStatus.Available,
            Version = Guid.NewGuid(),
            CreatedAt = registeredAt.ToUniversalTime()
        };
        device.UpdatedAt = device.CreatedAt;
        return Result<IoTDevice>.Success(device);
    }

    public bool IsAvailable() => Status == DeviceStatus.Available && _assignments.All(assignment => !assignment.IsOpen());

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

    public Result<DeviceAssignment> AssignTo(Guid targetId, DateTimeOffset at)
    {
        if (!IsAvailable() || _assignments.Any(assignment => assignment.EndedAt > at))
        {
            return Result<DeviceAssignment>.Failure(IoTAssetsErrors.InvalidState);
        }

        if (targetId == Guid.Empty || at == default)
        {
            return Result<DeviceAssignment>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var targetType = Type == DeviceType.SmartCollar ? AssignmentTargetType.Cattle : AssignmentTargetType.WaterTrough;
        var assignment = new DeviceAssignment(Id, RanchId, targetId, targetType, at);
        _assignments.Add(assignment);
        Status = DeviceStatus.InService;
        Touch(at);
        AddDomainEvent(new DeviceAssignedEvent(RanchId, Id, assignment.Id, targetId, targetType));
        return Result<DeviceAssignment>.Success(assignment);
    }

    public Result<DeviceAssignment> Release(string reason, DateTimeOffset at)
    {
        var assignment = _assignments.SingleOrDefault(item => item.IsOpen());
        if (assignment is null)
        {
            return Result<DeviceAssignment>.Failure(IoTAssetsErrors.InvalidState);
        }

        var closure = assignment.Close(reason, at);
        if (closure.IsFailure)
        {
            return Result<DeviceAssignment>.Failure(closure.DomainError);
        }

        Status = DeviceStatus.Available;
        Touch(at);
        AddDomainEvent(new DeviceReleasedEvent(RanchId, Id, assignment.Id, assignment.TargetId, assignment.TargetType,
            assignment.ReleaseReason!));
        return Result<DeviceAssignment>.Success(assignment);
    }

    public Result SetMaintenance(bool inMaintenance, DateTimeOffset at)
    {
        if (_assignments.Any(assignment => assignment.IsOpen()) || at == default)
        {
            return Result.Failure(IoTAssetsErrors.InvalidState);
        }

        var status = inMaintenance ? DeviceStatus.Maintenance : DeviceStatus.Available;
        if (Status != status)
        {
            Status = status;
            Touch(at);
        }

        return Result.Success();
    }

    public Result RegisterCommunication(DateTimeOffset communicatedAt, DateTimeOffset receivedAt,
        BatteryLevel? battery, DateTimeOffset now)
    {
        if (communicatedAt == default || receivedAt == default || communicatedAt > receivedAt || receivedAt > now)
        {
            return Result.Failure(IoTAssetsErrors.InvalidInput);
        }

        if (battery is not null && BatteryRecordedAt == communicatedAt && Battery != battery)
        {
            return Result.Failure(IoTAssetsErrors.Conflict);
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

        return Result.Success();
    }

    public Result EnsureVersion(Guid expectedVersion)
    {
        if (expectedVersion == Guid.Empty)
        {
            return Result.Failure(IoTAssetsErrors.InvalidInput);
        }

        return Version == expectedVersion
            ? Result.Success()
            : Result.Failure(IoTAssetsErrors.Conflict);
    }

    private void Touch(DateTimeOffset at)
    {
        Version = Guid.NewGuid();
        UpdatedAt = at.ToUniversalTime();
    }
}
