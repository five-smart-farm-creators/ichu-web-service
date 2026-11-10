using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

public sealed class RanchMembership
{
    // Entity Framework Core materializes memberships without running the factory.
    private RanchMembership()
    {
    }

    public Guid Id { get; private set; }

    public UserAccountId UserAccountId { get; private set; }

    public Guid RanchId { get; private set; }

    public bool IsOwner { get; private set; }

    public static Result<RanchMembership> Create(UserAccountId userAccountId, Guid ranchId, bool isOwner)
    {
        if (userAccountId.Value == Guid.Empty || ranchId == Guid.Empty)
        {
            return Result<RanchMembership>.Failure(IamErrors.InvalidIdentifier);
        }

        return Result<RanchMembership>.Success(new RanchMembership
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            RanchId = ranchId,
            IsOwner = isOwner
        });
    }
}
