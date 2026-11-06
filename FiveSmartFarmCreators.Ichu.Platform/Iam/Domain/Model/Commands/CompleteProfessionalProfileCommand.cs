namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries the details required to complete a veterinarian profile.
/// </summary>
/// <param name="Specialty">
///     The veterinarian's professional specialty.
/// </param>
/// <param name="LicenseNumber">
///     The unique professional license number.
/// </param>
/// <param name="YearsOfExperience">
///     The veterinarian's years of experience.
/// </param>
public sealed record CompleteProfessionalProfileCommand(string Specialty, string LicenseNumber, int YearsOfExperience);
