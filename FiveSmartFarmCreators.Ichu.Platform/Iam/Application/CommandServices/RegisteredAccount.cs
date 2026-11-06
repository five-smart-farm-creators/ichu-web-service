using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>
///     Carries the newly created account and its optional ranch identifier.
/// </summary>
public sealed record RegisteredAccount(UserAccount Account, Guid? RanchId);
