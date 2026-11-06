using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the AccountResource REST representation.</summary>
public sealed record AccountResource(Guid Id, string Email, string Role, string Status, ProfessionalProfileResource? Profile);
