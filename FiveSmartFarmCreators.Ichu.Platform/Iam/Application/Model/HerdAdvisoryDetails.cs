using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;

/// <summary>Projects advisory history and professional data without changing persisted state.</summary>
public sealed record HerdAdvisoryDetails(Guid Id, Guid VeterinarianId, Guid RanchId, string Reason,
    AdvisoryStatus Status, AccessScope Scope, DateTimeOffset RequestedAt, DateTimeOffset? GrantedAt,
    DateTimeOffset? ExpiresAt, DateTimeOffset? EndedAt, ProfessionalProfile? VeterinarianProfile);
