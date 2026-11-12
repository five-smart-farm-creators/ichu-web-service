using System.Security.Cryptography;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Security;

/// <summary>
///     Draws from the operating system's cryptographic generator, so that a code cannot be predicted from earlier codes.
/// </summary>
public sealed class RandomRanchAccessCodeGenerator : IRanchAccessCodeGenerator
{
    public RanchAccessCode Next()
    {
        var symbols = new char[RanchAccessCode.Length];
        for (var index = 0; index < symbols.Length; index++)
        {
            symbols[index] = RanchAccessCode.Alphabet[RandomNumberGenerator.GetInt32(RanchAccessCode.Alphabet.Length)];
        }

        return new RanchAccessCode(new string(symbols));
    }
}
