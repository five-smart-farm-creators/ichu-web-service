using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Security;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Infrastructure;

public class RandomRanchAccessCodeGeneratorTests
{
    [Fact]
    public void Next_DrawsCodesOfTheConfiguredLengthFromTheAlphabet()
    {
        var generator = new RandomRanchAccessCodeGenerator();

        for (var draw = 0; draw < 1000; draw++)
        {
            var code = generator.Next().Value;

            Assert.Equal(RanchAccessCode.Length, code.Length);
            Assert.All(code, symbol => Assert.Contains(symbol, RanchAccessCode.Alphabet));
        }
    }
}
