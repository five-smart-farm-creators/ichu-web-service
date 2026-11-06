namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record RegisterDeviceCommunicationCommand(Guid RanchId, Guid DeviceId, DateTimeOffset CommunicatedAt, DateTimeOffset ReceivedAt,
    int? BatteryPercentage);
