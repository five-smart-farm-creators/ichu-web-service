using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Interfaces.Rest;

public class DashboardAnalyticsActionResultAssemblerTests
{
    private static readonly IStringLocalizer<DashboardAnalyticsMessages> Localizer = new ServiceCollection()
        .AddLogging()
        .AddLocalization()
        .BuildServiceProvider()
        .GetRequiredService<IStringLocalizer<DashboardAnalyticsMessages>>();

    [Fact]
    public void InvalidInputAnswersBadRequest() => AssertProblem(DashboardAnalyticsErrors.InvalidPeriod, StatusCodes.Status400BadRequest);

    [Fact]
    public void ForbiddenRanchAnswersForbidden() => AssertProblem(DashboardAnalyticsErrors.RanchAccessDenied, StatusCodes.Status403Forbidden);

    [Fact]
    public void ExportOutsideTheSubscriptionAnswersForbidden() => AssertProblem(DashboardAnalyticsErrors.ExportNotIncluded, StatusCodes.Status403Forbidden);

    [Fact]
    public void ExportWithoutDataAnswersUnprocessableEntity() =>
        AssertProblem(DashboardAnalyticsErrors.NoDataToExport, StatusCodes.Status422UnprocessableEntity);

    [Fact]
    public void InconsistentSourceAnswersServiceUnavailable() =>
        AssertProblem(DashboardAnalyticsErrors.SourceUnavailable, StatusCodes.Status503ServiceUnavailable);

    private static void AssertProblem(Error error, int statusCode)
    {
        var controller = new TestController { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

        var response = Assert.IsType<ObjectResult>(DashboardAnalyticsActionResultAssembler.ToProblem(controller, error, Localizer));

        Assert.Equal(statusCode, response.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(response.Value);
        Assert.Equal(error.Code, Assert.IsType<string>(problem.Extensions["code"]));
    }

    private sealed class TestController : ControllerBase;
}
