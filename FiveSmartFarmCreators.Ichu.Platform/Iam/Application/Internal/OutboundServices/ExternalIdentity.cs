using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;

public sealed record ExternalIdentity(string Uid, Email Email, bool EmailVerified);
