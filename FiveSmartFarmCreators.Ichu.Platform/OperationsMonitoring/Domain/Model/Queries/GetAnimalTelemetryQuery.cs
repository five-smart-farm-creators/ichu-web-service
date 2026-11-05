using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

public sealed record GetAnimalTelemetryQuery(Guid AnimalId, TimePeriod Period, int Page, int PageSize);
