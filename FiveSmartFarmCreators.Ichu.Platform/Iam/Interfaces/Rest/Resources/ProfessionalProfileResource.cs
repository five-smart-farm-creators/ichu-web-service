namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the professional profile details exposed for a veterinarian.
/// </summary>
public sealed record ProfessionalProfileResource(string Specialty, string LicenseNumber, int YearsOfExperience);
