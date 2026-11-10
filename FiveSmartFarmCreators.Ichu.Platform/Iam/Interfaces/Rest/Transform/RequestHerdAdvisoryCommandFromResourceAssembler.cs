using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RequestHerdAdvisoryCommandFromResourceAssembler
{
    public static RequestHerdAdvisoryCommand ToCommandFromResource(Guid ranchId, RequestAdvisoryResource resource) =>
        new(ranchId, resource.Reason);
}
