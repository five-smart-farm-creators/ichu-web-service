using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

public static class RegisterAccountCommandFromResourceAssembler
{
    public static Result<RegisterAccountCommand> ToCommandFromResource(RegisterAccountResource resource) =>
        Enum.TryParse<UserRole>(resource.Role, true, out var role) && Enum.IsDefined(role)
            ? Result<RegisterAccountCommand>.Success(new RegisterAccountCommand(role, resource.RanchName, resource.Region,
                resource.District, resource.EstimatedHeadCount))
            : Result<RegisterAccountCommand>.Failure(IamErrors.InvalidRole);
}
