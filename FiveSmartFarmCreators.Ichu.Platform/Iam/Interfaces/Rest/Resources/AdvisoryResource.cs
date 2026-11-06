namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the AdvisoryResource REST representation.</summary>
public sealed record AdvisoryResource(Guid Id, Guid VeterinarianId, Guid RanchId, string Reason, string Status, string Scope, DateTimeOffset RequestedAt, DateTimeOffset? GrantedAt, DateTimeOffset? ExpiresAt, DateTimeOffset? EndedAt, ProfessionalProfileResource? VeterinarianProfile);
