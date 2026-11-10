namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

public sealed record CompleteProfessionalProfileCommand(string Specialty, string LicenseNumber, int YearsOfExperience);
