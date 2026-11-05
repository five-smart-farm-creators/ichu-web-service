using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>The outcome of an attention request together with the alert state that is currently stored.</summary>
public sealed record AlertAttentionResult(AlertChangeOutcome Outcome, MonitoringAlert Alert);
