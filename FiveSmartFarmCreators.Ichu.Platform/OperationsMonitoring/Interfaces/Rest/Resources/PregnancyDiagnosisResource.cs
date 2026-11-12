namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record PregnancyDiagnosisResource(DateTimeOffset OccurredAt, string Result, DateOnly? ExpectedCalvingDate);
