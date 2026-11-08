using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Resources;

public class PlanningMessagesTests
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
        Assert.Equal("The animal was not found in this ranch.", PlanningErrors.CattleNotFound.Message);
    }

    [Fact]
    public void EveryErrorHasItsOwnMessageInsteadOfFallingBackToItsCode()
    {
        var errors = typeof(PlanningErrors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (Error)field.GetValue(null)!);

        Assert.All(errors, error => Assert.NotEqual(error.Code, error.Message));
    }

    private static List<string> ResourceKeys(CultureInfo culture)
    {
        var resources = new ResourceManager(typeof(PlanningMessages));
        using var resourceSet = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"The {culture.Name} resource file is missing.");
        return resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToList();
    }
}
