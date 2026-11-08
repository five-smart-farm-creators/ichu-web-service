using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Resources;

public class IoTAssetsMessagesTests
{
    [Fact]
    public void SpanishFileTranslatesEveryEnglishMessage()
    {
        var english = ResourceKeys(CultureInfo.InvariantCulture);
        var spanish = ResourceKeys(new CultureInfo("es"));

        Assert.Equal(english.Order(), spanish.Order());
    }

    [Fact]
    public void ErrorCodesKeepTheirStableIdentifiers()
    {
        Assert.Equal("IoTAssets.DeviceLimitReached", IoTAssetsErrors.DeviceLimitReached.Code);
        Assert.Equal("The assigned collar limit of the current plan has been reached.", IoTAssetsErrors.DeviceLimitReached.Message);
    }

    [Fact]
    public void EveryErrorHasItsOwnMessageInsteadOfFallingBackToItsCode()
    {
        var errors = typeof(IoTAssetsErrors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (Error)field.GetValue(null)!);

        Assert.All(errors, error => Assert.NotEqual(error.Code, error.Message));
    }

    private static List<string> ResourceKeys(CultureInfo culture)
    {
        var resources = new ResourceManager(typeof(IoTAssetsMessages));
        using var resourceSet = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"The {culture.Name} resource file is missing.");
        return resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToList();
    }
}
