namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record AccountResource(Guid Id, string Email, string Role, string Status, ProfessionalProfileResource? Profile);
