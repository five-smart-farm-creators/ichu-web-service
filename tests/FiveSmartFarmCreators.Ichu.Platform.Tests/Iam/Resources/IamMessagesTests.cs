using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Resources;

public class IamMessagesTests
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
        Assert.Equal("An account already exists for this identity.", IamErrors.AccountAlreadyExists.Message);
    }

    [Fact]
    public void EveryErrorHasItsOwnMessageAndAnIamCode()
    {
        var errors = typeof(IamErrors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (Error)field.GetValue(null)!);

        Assert.All(errors, error =>
        {
            Assert.StartsWith("Iam.", error.Code);
            Assert.NotEqual(error.Code, error.Message);
        });
    }

    private static List<string> ResourceKeys(CultureInfo culture)
    {
        var resources = new ResourceManager(typeof(IamMessages));
        using var resourceSet = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"The {culture.Name} resource file is missing.");
        return resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToList();
    }
}
