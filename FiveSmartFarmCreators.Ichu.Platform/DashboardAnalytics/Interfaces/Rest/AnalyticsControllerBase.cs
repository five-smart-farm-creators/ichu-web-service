using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Applies Firebase authentication and analytics error handling to derived REST controllers.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "Firebase")]
[TypeFilter(typeof(AnalyticsExceptionFilter))]
public abstract class AnalyticsControllerBase : ControllerBase;
