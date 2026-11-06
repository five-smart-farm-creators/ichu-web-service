using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the RegisterAccountResource REST representation.</summary>
public sealed record RegisterAccountResource([Required] string Role, string? RanchName, string? Region, string? District, int? EstimatedHeadCount);
