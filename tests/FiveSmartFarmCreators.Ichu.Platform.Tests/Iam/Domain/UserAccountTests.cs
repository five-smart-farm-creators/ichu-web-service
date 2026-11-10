using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Domain;

public class UserAccountTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_StartsPendingVerification()
    {
        var account = NewAccount(UserRole.RanchManager);

        Assert.Equal(AccountStatus.PendingVerification, account.Status);
        Assert.False(account.IsActive(Now));
    }

    [Fact]
    public void Create_WithBlankExternalUid_ReturnsInvalidExternalIdentity()
    {
        var result = UserAccount.Create(" ", new Email("vet@example.com"), UserRole.Veterinarian, Now);

        Assert.Equal(IamErrors.InvalidExternalIdentity, result.DomainError);
    }

    [Fact]
    public void Create_WithUndefinedRole_ReturnsInvalidExternalIdentity()
    {
        var result = UserAccount.Create("uid-1", new Email("vet@example.com"), (UserRole)99, Now);

        Assert.Equal(IamErrors.InvalidExternalIdentity, result.DomainError);
    }

    [Fact]
    public void RecordFailedAttempt_FifthFailure_BlocksTheAccountForFifteenMinutes()
    {
        var account = VerifiedAccount(UserRole.Veterinarian);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            account.RecordFailedAttempt(Now);
        }

        Assert.True(account.IsBlocked(Now.AddMinutes(14)));
        Assert.False(account.IsBlocked(Now.AddMinutes(15)));
        Assert.Equal(AccountStatus.TemporarilyBlocked, account.Status);
    }

    [Fact]
    public void RecordSuccessfulAccess_WhileBlocked_ReturnsAccountNotActive()
    {
        var account = VerifiedAccount(UserRole.Veterinarian);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            account.RecordFailedAttempt(Now);
        }

        var result = account.RecordSuccessfulAccess(Now.AddMinutes(1));

        Assert.Equal(IamErrors.AccountNotActive, result.DomainError);
    }

    [Fact]
    public void RecordSuccessfulAccess_ForVerifiedAccount_ClearsTheFailureHistory()
    {
        var account = VerifiedAccount(UserRole.Veterinarian);
        account.RecordFailedAttempt(Now);

        var result = account.RecordSuccessfulAccess(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, account.FailedAttempts);
    }

    [Fact]
    public void CompleteProfile_ForRanchManager_ReturnsProfileOnlyForVeterinarians()
    {
        var account = VerifiedAccount(UserRole.RanchManager);
        var profile = ProfessionalProfile.Create("Bovine health", "CMVP-1", 5).Value!;

        var result = account.CompleteProfile(profile);

        Assert.Equal(IamErrors.ProfileOnlyForVeterinarians, result.DomainError);
        Assert.Null(account.Profile);
    }

    [Fact]
    public void CompleteProfile_ForVeterinarian_AssignsTheProfile()
    {
        var account = VerifiedAccount(UserRole.Veterinarian);
        var profile = ProfessionalProfile.Create("Bovine health", "cmvp-1", 5).Value!;

        var result = account.CompleteProfile(profile);

        Assert.True(result.IsSuccess);
        Assert.Equal("CMVP-1", account.Profile!.LicenseNumber);
    }

    private static UserAccount NewAccount(UserRole role) =>
        UserAccount.Create("uid-1", new Email("vet@example.com"), role, Now).Value!;

    private static UserAccount VerifiedAccount(UserRole role)
    {
        var account = NewAccount(role);
        account.Verify();
        return account;
    }
}
