namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>Records the professional information declared by a veterinarian.</summary>
public sealed class ProfessionalProfile
{
    private ProfessionalProfile() { }

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
