using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the requested account role and optional ranch registration details.
/// </summary>
public sealed record RegisterAccountResource([Required] string Role, string? RanchName, string? Region, string? District, int? EstimatedHeadCount);
