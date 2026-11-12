namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record ReproductiveServiceResource(DateTimeOffset OccurredAt, string Method, string? SireReference);
