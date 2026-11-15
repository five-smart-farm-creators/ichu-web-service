using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

public sealed record WaterTroughReadingReceipt(WaterTroughReading Reading, bool WasCreated);
