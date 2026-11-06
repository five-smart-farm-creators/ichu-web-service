using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the ranch and reason for a veterinarian's access request.
/// </summary>
public sealed record RequestAdvisoryResource(Guid RanchId, [Required, StringLength(1000)] string Reason);
