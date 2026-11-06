using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class RequestHerdAdvisoryCommandFromResourceAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static RequestHerdAdvisoryCommand ToCommandFromResource(RequestAdvisoryResource resource) =>
        new(resource.RanchId, resource.Reason);
}
