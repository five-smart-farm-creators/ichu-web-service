using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the Request Advisory Resource contract.</summary>
public sealed record RequestAdvisoryResource(Guid RanchId, [Required, StringLength(1000)] string Reason);
