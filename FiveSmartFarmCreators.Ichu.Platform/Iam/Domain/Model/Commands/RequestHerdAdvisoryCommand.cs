namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>Defines the input for RequestHerdAdvisory.</summary>
public sealed record RequestHerdAdvisoryCommand(Guid RanchId, string Reason);
