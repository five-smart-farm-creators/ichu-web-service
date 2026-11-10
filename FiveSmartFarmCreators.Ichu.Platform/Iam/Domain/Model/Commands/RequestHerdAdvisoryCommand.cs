namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

public sealed record RequestHerdAdvisoryCommand(Guid RanchId, string Reason);
