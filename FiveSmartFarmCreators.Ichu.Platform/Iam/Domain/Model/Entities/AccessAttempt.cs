using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>
///     Records whether an access attempt succeeded, without storing credentials.
/// </summary>
public sealed class AccessAttempt
{
    // Entity Framework Core materializes attempts without running the factory.
    private AccessAttempt()
    {
    }

    public Guid Id { get; private set; }

    public UserAccountId UserAccountId { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    public bool Succeeded { get; private set; }

    public static Result<AccessAttempt> Create(UserAccountId userAccountId, DateTimeOffset recordedAt, bool succeeded)
    {
        if (userAccountId.Value == Guid.Empty)
        {
            return Result<AccessAttempt>.Failure(IamErrors.InvalidIdentifier);
        }

        return Result<AccessAttempt>.Success(new AccessAttempt
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            RecordedAt = recordedAt,
            Succeeded = succeeded
        });
    }
}
