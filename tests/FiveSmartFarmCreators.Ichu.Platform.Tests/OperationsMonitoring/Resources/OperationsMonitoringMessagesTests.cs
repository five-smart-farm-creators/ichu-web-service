using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Resources;

public class OperationsMonitoringMessagesTests
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
        Assert.Equal("The alert does not exist.", OperationsMonitoringErrors.AlertNotFound.Message);
    }

    [Fact]
    public void EveryErrorHasItsOwnMessageInsteadOfFallingBackToItsCode()
    {
        var errors = Errors();

        Assert.All(errors, error => Assert.NotEqual(error.Code, error.Message));
    }

    [Fact]
    public void EveryErrorCodeIsPrefixedByTheContext()
    {
        Assert.All(Errors(), error => Assert.StartsWith("OperationsMonitoring.", error.Code));
    }

    private static IEnumerable<Error> Errors() =>
        typeof(OperationsMonitoringErrors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (Error)field.GetValue(null)!);

    private static List<string> ResourceKeys(CultureInfo culture)
    {
        var resources = new ResourceManager(typeof(OperationsMonitoringMessages));
        using var resourceSet = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"The {culture.Name} resource file is missing.");
        return resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).ToList();
    }
}
