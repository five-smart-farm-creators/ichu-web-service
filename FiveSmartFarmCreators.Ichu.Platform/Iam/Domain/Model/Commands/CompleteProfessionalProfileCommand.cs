namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>Defines the input for CompleteProfessionalProfile.</summary>
public sealed record CompleteProfessionalProfileCommand(string Specialty, string LicenseNumber, int YearsOfExperience);
