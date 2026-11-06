using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;

/// <summary>
///     Replaces the <c>[controller]</c> route token with the kebab-case controller name, so that routes follow
///     the plural lowercase kebab-case resource names required by the API conventions.
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    /// <summary>
    ///     Applies the convention to the routes of a controller and its actions.
    /// </summary>
    /// <param name="controller">The controller model to update.</param>
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
        }

        foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
        {
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
        }
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel is null
            ? null
            : new AttributeRouteModel
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            };
    }
}
