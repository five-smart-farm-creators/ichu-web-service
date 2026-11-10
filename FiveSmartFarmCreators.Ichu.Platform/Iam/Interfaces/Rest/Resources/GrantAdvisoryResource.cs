using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record GrantAdvisoryResource(Guid VeterinarianId, [Required] string Scope, DateTimeOffset ExpiresAt);
