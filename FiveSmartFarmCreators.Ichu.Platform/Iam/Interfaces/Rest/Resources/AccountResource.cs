using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the account identity, role, status, and optional profile exposed by the API.
/// </summary>
public sealed record AccountResource(Guid Id, string Email, string Role, string Status, ProfessionalProfileResource? Profile);
