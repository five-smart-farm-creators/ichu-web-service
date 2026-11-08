using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

/// <summary>
///     A drinking place of the ranch that can carry a temperature band. Registering it does not use a collar.
/// </summary>
public partial class WaterTrough : AggregateRoot
{
    // Entity Framework Core materializes water troughs without running the factory.
    private WaterTrough()
    {
    }

    public WaterTroughId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid? LotId { get; private set; }

    public TemperatureRange? TemperatureRange { get; private set; }

    public Guid Version { get; private set; }

    public static Result<WaterTrough> Register(RegisterWaterTroughCommand command, DateTimeOffset registeredAt)
    {
        if (command.RanchId == Guid.Empty || command.LotId == Guid.Empty || registeredAt == default
            || string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 80)
        {
            return Result<WaterTrough>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var waterTrough = new WaterTrough
        {
            Id = new WaterTroughId(Guid.NewGuid()),
            RanchId = command.RanchId,
            Name = command.Name.Trim(),
            LotId = command.LotId,
            Version = Guid.NewGuid(),
            CreatedAt = registeredAt.ToUniversalTime()
        };
        waterTrough.UpdatedAt = waterTrough.CreatedAt;
        return Result<WaterTrough>.Success(waterTrough);
    }

    public Result ConfigureTemperatureRange(TemperatureRange range, Guid expectedVersion, DateTimeOffset at)
    {
        if (expectedVersion == Guid.Empty || at == default)
        {
            return Result.Failure(IoTAssetsErrors.InvalidInput);
        }

        if (expectedVersion != Version)
        {
            return Result.Failure(IoTAssetsErrors.Conflict);
        }

        TemperatureRange = range;
        Version = Guid.NewGuid();
        UpdatedAt = at.ToUniversalTime();
        return Result.Success();
    }
}
