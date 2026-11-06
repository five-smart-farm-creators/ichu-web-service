using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the ranch, veterinarian, scope, and expiration for a direct access grant.
/// </summary>
public sealed record GrantAdvisoryResource(Guid RanchId, Guid VeterinarianId, [Required] string Scope, DateTimeOffset ExpiresAt);
