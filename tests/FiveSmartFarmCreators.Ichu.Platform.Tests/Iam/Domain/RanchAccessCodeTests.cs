using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Domain;

public class RanchAccessCodeTests
{
    [Fact]
    public void Create_TrimsAndUppercasesTheCode()
    {
        var result = RanchAccessCode.Create("  abcd2345 ");

        Assert.Equal("ABCD2345", result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ABCDEFG")]
    [InlineData("ABCDEFGHJ")]
    public void Create_RejectsCodesThatAreNotEightCharacters(string value)
    {
        var result = RanchAccessCode.Create(value);

        Assert.Equal(IamErrors.InvalidRanchAccessCode, result.DomainError);
    }

    [Theory]
    [InlineData("ABCDEFG0")]
    [InlineData("ABCDEFG1")]
    [InlineData("ABCDEFGI")]
    [InlineData("ABCDEFGo")]
    public void Create_RejectsAmbiguousCharacters(string value)
    {
        var result = RanchAccessCode.Create(value);

        Assert.Equal(IamErrors.InvalidRanchAccessCode, result.DomainError);
    }
}
