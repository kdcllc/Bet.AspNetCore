using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
#nullable disable
    public class KebabRoutingConvention : IControllerModelConvention
    {
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

            static AttributeRouteModel ReplaceControllerTemplate(SelectorModel selector, string name)
            {
                if (selector.AttributeRouteModel == null)
                {
                    return null;
                }

                var template = selector.AttributeRouteModel.Template;

                if (selector.AttributeRouteModel != null
                    && template != null
                    && template.IndexOf("[controller]", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    template = selector.AttributeRouteModel.Template.Replace("[controller]", PascalToKebabCase(name));
                }

                selector.AttributeRouteModel.Template = template;

                return selector.AttributeRouteModel;
            }

            static string PascalToKebabCase(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }

                return Regex.Replace(
                    value,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "-$1",
                    RegexOptions.Compiled)
                    .Trim()
                    .ToLower(CultureInfo.CurrentCulture);
            }
        }
    }
#nullable restore
}
