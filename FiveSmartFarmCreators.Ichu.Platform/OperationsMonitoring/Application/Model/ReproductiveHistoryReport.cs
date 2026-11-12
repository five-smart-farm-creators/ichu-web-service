using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     The reproductive history of an animal with the facts that place it in time: its age in whole months and its productive stage.
/// </summary>
public sealed record ReproductiveHistoryReport(CattleProfileSnapshot Animal, int AgeInMonths, ReproductiveHistory History);
