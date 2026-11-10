using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record RequestAdvisoryResource([Required, StringLength(1000)] string Reason);
