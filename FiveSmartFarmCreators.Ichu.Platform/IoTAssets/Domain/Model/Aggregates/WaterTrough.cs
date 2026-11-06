using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

public partial class WaterTrough : AggregateRoot
{
    private WaterTrough()
    {
    }

    public WaterTrough(RegisterWaterTroughCommand command, DateTimeOffset registeredAt)
    {
        if (command.RanchId == Guid.Empty || command.LotId == Guid.Empty || registeredAt == default
            || string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 80)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        Id = Guid.NewGuid();
        RanchId = command.RanchId;
        Name = command.Name.Trim();
        LotId = command.LotId;
        Version = Guid.NewGuid();
        CreatedAt = registeredAt.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid? LotId { get; private set; }
    public TemperatureRange? TemperatureRange { get; private set; }
    public Guid Version { get; private set; }

    public void ConfigureTemperatureRange(TemperatureRange range, Guid expectedVersion, DateTimeOffset at)
    {
        if (range is null || expectedVersion == Guid.Empty || at == default)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        if (expectedVersion != Version)
        {
            throw new IoTAssetsException(IoTAssetsErrors.Conflict);
        }

        TemperatureRange = range;
        Version = Guid.NewGuid();
        UpdatedAt = at.ToUniversalTime();
    }
}
