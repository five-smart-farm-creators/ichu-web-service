using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Seeding;

/// <summary>
///     Creates the Development ranch and the Firebase user who manages it, so that a real account can sign in to the
///     Development API and see the seeded data. Nothing is created unless both identity settings are configured.
/// </summary>
public sealed class DevelopmentIamSeeder(
    IUserAccountRepository accounts,
    IRanchRegistrationRepository ranches,
    AppDbContext context,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ILogger<DevelopmentIamSeeder> logger,
    TimeProvider clock)
{
    private const string ExternalUidSetting = "DevelopmentSeed:Manager:ExternalUid";
    private const string EmailSetting = "DevelopmentSeed:Manager:Email";

    /// <summary>
    ///     Skips the access check because no HTTP account exists at startup. The account stays pending until its first
    ///     request with a verified email activates it, as it does for any other account.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var externalUid = configuration[ExternalUidSetting];
        var email = configuration[EmailSetting];
        if (string.IsNullOrWhiteSpace(externalUid) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation(
                "The Development demo account is skipped. Set DevelopmentSeed__Manager__ExternalUid and DevelopmentSeed__Manager__Email to create it.");
            return;
        }

        if (!await ranches.ExistsAsync(DevelopmentSeed.RanchId, cancellationToken))
        {
            await AddRanchAsync(cancellationToken);
        }

        var account = await accounts.FindByExternalUidAsync(externalUid, cancellationToken)
            ?? await AddAccountAsync(externalUid, email, cancellationToken);
        var memberships = await ranches.FindMembershipsByAccountAsync(account.Id, cancellationToken);
        if (memberships.All(membership => membership.RanchId != DevelopmentSeed.RanchId))
        {
            var ownership = Unwrap(RanchMembership.Create(account.Id, DevelopmentSeed.RanchId, isOwner: true));
            await ranches.AddMembershipAsync(ownership, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task AddRanchAsync(CancellationToken cancellationToken)
    {
        var accessCode = Unwrap(RanchAccessCode.Create(DevelopmentSeed.RanchAccessCode));
        var ranch = Unwrap(RanchRegistration.Create("Development ranch", "Development region", "Development district", 2, accessCode));
        // Development-only: the other seeders and the edge's seed reference the ranch by this fixed identifier.
        context.Entry(ranch).Property(entry => entry.Id).CurrentValue = DevelopmentSeed.RanchId;
        await ranches.AddAsync(ranch, cancellationToken);
    }

    private async Task<UserAccount> AddAccountAsync(string externalUid, string email, CancellationToken cancellationToken)
    {
        var address = Unwrap(Email.Create(email));
        var account = Unwrap(UserAccount.Create(externalUid, address, UserRole.RanchManager, clock.GetUtcNow()));
        await accounts.AddAsync(account, cancellationToken);
        return account;
    }

    private static T Unwrap<T>(Result<T> result) =>
        result.IsFailure ? throw new InvalidOperationException(result.DomainError.Message) : result.Value!;
}
