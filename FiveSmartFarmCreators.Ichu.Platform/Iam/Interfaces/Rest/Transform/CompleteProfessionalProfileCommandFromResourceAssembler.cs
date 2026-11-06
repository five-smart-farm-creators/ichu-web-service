using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class CompleteProfessionalProfileCommandFromResourceAssembler
{
    public static CompleteProfessionalProfileCommand ToCommandFromResource(CompleteProfileResource resource) =>
        new(resource.Specialty, resource.LicenseNumber, resource.YearsOfExperience);
}
