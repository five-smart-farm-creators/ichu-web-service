using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record RegisterAccountResource([Required] string Role, string? RanchName, string? Region, string? District, int? EstimatedHeadCount);
