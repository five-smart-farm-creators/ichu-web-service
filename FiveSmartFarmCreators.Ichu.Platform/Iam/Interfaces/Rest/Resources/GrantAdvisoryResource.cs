using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the Grant Advisory Resource contract.</summary>
public sealed record GrantAdvisoryResource(Guid RanchId, Guid VeterinarianId, [Required] string Scope, DateTimeOffset ExpiresAt);
