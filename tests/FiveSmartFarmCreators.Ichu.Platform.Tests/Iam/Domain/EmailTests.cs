using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Domain;

public class EmailTests
{
    [Fact]
    public void Create_TrimsAndLowercasesTheAddress()
    {
        var result = Email.Create("  Manager@Example.COM ");

        Assert.Equal("manager@example.com", result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-an-email")]
    public void Create_RejectsAddressesThatAreNotValid(string value)
    {
        var result = Email.Create(value);

        Assert.Equal(IamErrors.InvalidEmail, result.DomainError);
    }
}
