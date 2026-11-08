using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record RegisterDeviceCommunicationCommand(
    Guid RanchId,
    DeviceId DeviceId,
    DateTimeOffset CommunicatedAt,
    DateTimeOffset ReceivedAt,
    int? BatteryPercentage);
