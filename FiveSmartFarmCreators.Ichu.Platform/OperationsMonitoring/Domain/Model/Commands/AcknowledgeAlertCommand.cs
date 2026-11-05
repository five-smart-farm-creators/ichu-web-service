namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record AcknowledgeAlertCommand(Guid AlertId, Guid OperationId, string Finding, string? Action, int ExpectedVersion);
