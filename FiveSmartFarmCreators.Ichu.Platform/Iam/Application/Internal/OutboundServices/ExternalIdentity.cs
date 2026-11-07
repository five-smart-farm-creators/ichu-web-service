using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>
///     Carries identity data verified by the external identity provider.
/// </summary>
public sealed record ExternalIdentity(string Uid, Email Email, bool EmailVerified);
