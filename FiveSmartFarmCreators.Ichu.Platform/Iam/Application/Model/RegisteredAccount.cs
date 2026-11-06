using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;

/// <summary>Returns an application account with its optional newly registered ranch.</summary>
public sealed record RegisteredAccount(UserAccount Account, Guid? RanchId);
