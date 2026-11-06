using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains professional details submitted to complete the current veterinarian profile.
/// </summary>
public sealed record CompleteProfileResource([Required, StringLength(120)] string Specialty,
    [Required, StringLength(80)] string LicenseNumber, [Range(0, int.MaxValue)] int YearsOfExperience);
