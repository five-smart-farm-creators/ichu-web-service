using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

public sealed record GrantHerdAdvisoryCommand(Guid RanchId, Guid VeterinarianId, AccessScope Scope, DateTimeOffset ExpiresAt);
