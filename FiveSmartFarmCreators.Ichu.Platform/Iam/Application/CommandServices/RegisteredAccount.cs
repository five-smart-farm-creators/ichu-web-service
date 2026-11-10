using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

public sealed record RegisteredAccount(UserAccount Account, Guid? RanchId);
