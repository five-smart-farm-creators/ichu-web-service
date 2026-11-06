using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>Defines the input for GrantHerdAdvisory.</summary>
public sealed record GrantHerdAdvisoryCommand(Guid RanchId, Guid VeterinarianId, AccessScope Scope, DateTimeOffset ExpiresAt);
