using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Resources;

public class SubscriptionPlansMessagesTests
{
    [Fact]
    public void SpanishFileTranslatesEveryEnglishMessage()
    {
        var english = ResourceKeys(CultureInfo.InvariantCulture);
        var spanish = ResourceKeys(new CultureInfo("es"));

        Assert.Equal(english.Order(), spanish.Order());
    }

    [Fact]
    public void ErrorMessageIsTheEnglishTextOfItsResource()
    {
        Assert.Equal("The subscription plan was not found.", SubscriptionPlansErrors.PlanNotFound.Message);
    }

    [Fact]
    public void EveryErrorHasItsOwnMessageInsteadOfFallingBackToItsCode()
    {
        var errors = typeof(SubscriptionPlansErrors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (Error)field.GetValue(null)!);

        Assert.All(errors, error => Assert.NotEqual(error.Code, error.Message));
    }

    private static List<string> ResourceKeys(CultureInfo culture)
    {
        var resources = new ResourceManager(typeof(SubscriptionPlansMessages));
        using var resourceSet = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"The {culture.Name} resource file is missing.");
        return resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToList();
    }
}
