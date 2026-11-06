using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RequestHerdAdvisoryCommandFromResourceAssembler
{
    public static RequestHerdAdvisoryCommand ToCommandFromResource(RequestAdvisoryResource resource) =>
        new(resource.RanchId, resource.Reason);
}
