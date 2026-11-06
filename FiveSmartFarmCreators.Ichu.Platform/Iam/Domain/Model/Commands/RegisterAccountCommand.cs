using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries the account role and optional ranch details for registration.
/// </summary>
/// <param name="Role">
///     The business role assigned to the new account.
/// </param>
/// <param name="RanchName">
///     The optional name of a ranch registered by its owner.
/// </param>
/// <param name="Region">
///     The optional region where the ranch is located.
/// </param>
/// <param name="District">
///     The optional district where the ranch is located.
/// </param>
/// <param name="EstimatedHeadCount">
///     The optional estimated number of animals.
/// </param>
public sealed record RegisterAccountCommand(UserRole Role, string? RanchName, string? Region, string? District, int? EstimatedHeadCount);
