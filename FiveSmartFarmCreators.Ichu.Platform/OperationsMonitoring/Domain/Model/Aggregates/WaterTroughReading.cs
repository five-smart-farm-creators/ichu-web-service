using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

public sealed class WaterTroughReading : AggregateRoot, IAuditableEntity
{
    public const string EdgeGatewayOrigin = "EdgeGateway";

    private WaterTroughReading() { }

    public Guid Id { get; private set; }
    public Guid ReadingId { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid WaterTroughId { get; private set; }
    public Guid RanchId { get; private set; }
    public Guid GatewayId { get; private set; }
    public decimal Temperature { get; private set; }
    public WaterTemperatureUnit TemperatureUnit { get; private set; }
    public bool IsValid { get; private set; }
    public WaterHeaterState HeaterState { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public string Origin { get; private set; } = EdgeGatewayOrigin;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public static Result<WaterTroughReading> Create(RegisterWaterControllerReadingCommand command)
    {
        if (command.GatewayId == Guid.Empty || command.RanchId == Guid.Empty || command.ControllerId == Guid.Empty
            || command.WaterTroughId == Guid.Empty || command.ReadingId == Guid.Empty
            || !Enum.IsDefined(command.TemperatureUnit) || !Enum.IsDefined(command.HeaterState)
            || command.CapturedAt == default || command.ReceivedAt == default
            || command.CapturedAt.UtcTicks % 10 != 0)
        {
            return Result<WaterTroughReading>.Failure(OperationsMonitoringErrors.WaterReadingInvalid);
        }

        var capturedAt = MonitoringTime.Normalize(command.CapturedAt);
        var receivedAt = MonitoringTime.Normalize(command.ReceivedAt);
        return Result<WaterTroughReading>.Success(new WaterTroughReading
        {
            Id = Guid.NewGuid(), ReadingId = command.ReadingId, ControllerId = command.ControllerId,
            WaterTroughId = command.WaterTroughId, RanchId = command.RanchId, GatewayId = command.GatewayId,
            Temperature = command.Temperature, TemperatureUnit = command.TemperatureUnit, IsValid = command.IsValid,
            HeaterState = command.HeaterState, CapturedAt = capturedAt, ReceivedAt = receivedAt,
            Origin = EdgeGatewayOrigin, CreatedAt = receivedAt, UpdatedAt = receivedAt
        });
    }

    public bool HasSameObservation(RegisterWaterControllerReadingCommand command) =>
        ControllerId == command.ControllerId && ReadingId == command.ReadingId
        && WaterTroughId == command.WaterTroughId && RanchId == command.RanchId
        && Temperature == command.Temperature && TemperatureUnit == command.TemperatureUnit
        && IsValid == command.IsValid && HeaterState == command.HeaterState
        && CapturedAt == MonitoringTime.Normalize(command.CapturedAt);
}
