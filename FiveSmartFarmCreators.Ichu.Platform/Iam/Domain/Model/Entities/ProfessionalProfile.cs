namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>
///     Stores the professional details declared by a veterinarian.
/// </summary>
public sealed class ProfessionalProfile
{
    private ProfessionalProfile() { }

    /// <summary>
    ///     Creates a profile, trims its text fields, and uppercases the license number.
    /// </summary>
    /// <param name="specialty">
    ///     The veterinarian's professional specialty.
    /// </param>
    /// <param name="licenseNumber">
    ///     The veterinarian's professional license number.
    /// </param>
    /// <param name="yearsOfExperience">
    ///     The veterinarian's years of experience.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     A required value is blank or too long, or the years of experience is negative.
    /// </exception>
    public ProfessionalProfile(string specialty, string licenseNumber, int yearsOfExperience)
    {
        if (string.IsNullOrWhiteSpace(specialty) || specialty.Length > 120 ||
            string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Length > 80 || yearsOfExperience < 0)
        {
            throw new ArgumentException("Specialty, license number and nonnegative experience are required.");
        }

        Specialty = specialty.Trim();
        LicenseNumber = licenseNumber.Trim().ToUpperInvariant();
        YearsOfExperience = yearsOfExperience;
    }

    public string Specialty { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public int YearsOfExperience { get; private set; }

}
