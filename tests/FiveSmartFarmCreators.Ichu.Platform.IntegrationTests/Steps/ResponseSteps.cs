using System.Text.Json;
using Reqnroll;
using Xunit;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Asserts the status, error code and fields of the last response, whichever steps produced it.
/// </summary>
[Binding]
public sealed class ResponseSteps(ApiScenarioContext context)
{
    [Then("the request returns status {int}")]
    public void ThenRequestReturnsStatus(int status) => AssertStatus(status);

    [Then("the request is refused with status {int}")]
    public void ThenRequestIsRefusedWithStatus(int status) => AssertStatus(status);

    [Then("the request is refused with status {int} and code {string}")]
    public void ThenRequestIsRefusedWithCode(int status, string code)
    {
        AssertStatus(status);
        Assert.Equal(code, context.RequireBody().GetProperty("code").GetString());
    }

    [Then("the response has:")]
    public void ThenResponseHas(DataTable table) => TableAssertions.AssertFields(context, context.RequireBody(), table);

    [Then("the response lists {int} items")]
    public void ThenResponseListsItems(int count)
    {
        var body = context.RequireBody();
        var items = body.ValueKind == JsonValueKind.Array ? body : body.GetProperty("items");
        Assert.Equal(count, items.GetArrayLength());
    }

    private void AssertStatus(int status) =>
        Assert.True((int)context.LastStatus == status,
            $"Expected HTTP {status} but received {(int)context.LastStatus}: {context.LastResponseText}");
}
