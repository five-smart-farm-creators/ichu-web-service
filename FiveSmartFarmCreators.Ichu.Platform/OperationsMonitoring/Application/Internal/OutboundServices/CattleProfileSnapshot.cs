using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     The animal facts that the reproductive rules and the reproductive history need from Cattle Information.
/// </summary>
public sealed record CattleProfileSnapshot(Guid CattleId, Guid RanchId, AnimalSex Sex, string LifeStage, DateOnly BirthDate);
