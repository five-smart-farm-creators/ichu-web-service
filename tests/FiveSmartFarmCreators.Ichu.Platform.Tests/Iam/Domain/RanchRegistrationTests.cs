using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Domain;

public class RanchRegistrationTests
{
    [Fact]
    public void Create_KeepsTheAccessCodeItWasGiven()
    {
        var accessCode = new RanchAccessCode("DEVRANCH");

        var ranch = RanchRegistration.Create("Los Andes", "Cusco", "Urubamba", 120, accessCode).Value!;

        Assert.Equal(accessCode, ranch.AccessCode);
    }
}
