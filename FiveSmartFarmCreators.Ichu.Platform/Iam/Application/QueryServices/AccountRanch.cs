using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>
///     The code is set only for a managing account, because anyone who holds it can request advisory access.
/// </summary>
public sealed record AccountRanch(RanchAccess Access, string Name, string Region, string District, RanchAccessCode? AccessCode);
