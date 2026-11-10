using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>
///     Stores the professional details declared by a veterinarian. The license number is stored in uppercase, so that its uniqueness ignores case.
/// </summary>
public sealed class ProfessionalProfile
{
    private const int MaximumSpecialtyLength = 120;
    private const int MaximumLicenseLength = 80;

    // Entity Framework Core materializes profiles without running the factory.
    private ProfessionalProfile()
    {
    }

    public string Specialty { get; private set; } = string.Empty;

    public string LicenseNumber { get; private set; } = string.Empty;

    public int YearsOfExperience { get; private set; }

    public static Result<ProfessionalProfile> Create(string specialty, string licenseNumber, int yearsOfExperience)
    {
        if (string.IsNullOrWhiteSpace(specialty) || specialty.Length > MaximumSpecialtyLength ||
            string.IsNullOrWhiteSpace(licenseNumber) || licenseNumber.Length > MaximumLicenseLength ||
            yearsOfExperience < 0)
        {
            return Result<ProfessionalProfile>.Failure(IamErrors.InvalidProfile);
        }

        return Result<ProfessionalProfile>.Success(new ProfessionalProfile
        {
            Specialty = specialty.Trim(),
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            YearsOfExperience = yearsOfExperience
        });
    }
}
