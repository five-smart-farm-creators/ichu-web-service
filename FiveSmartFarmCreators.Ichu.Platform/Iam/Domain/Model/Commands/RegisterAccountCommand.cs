using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

public sealed record RegisterAccountCommand(UserRole Role, string? RanchName, string? Region, string? District, int? EstimatedHeadCount);
