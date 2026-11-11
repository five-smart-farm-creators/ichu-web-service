using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;

/// <summary>
///     Draws candidate codes without touching storage, so the caller checks each candidate for uniqueness.
/// </summary>
public interface IRanchAccessCodeGenerator
{
    RanchAccessCode Next();
}
