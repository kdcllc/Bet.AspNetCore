using System.Collections.Generic;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bet.AspNetCore.Swagger.OperationFilters
{
    /// <summary>
    /// Allows to add custom input for the custom headers in Swagger interface.
    /// </summary>
    public class AddHeaderOperationFilter : IOperationFilter
    {
        private readonly string _description;
        private readonly string _parameterName;
        private readonly bool _required;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddHeaderOperationFilter"/> class.
        /// </summary>
        /// <code>
        /// c.OperationFilter&lt;AddHeaderOperationFilter&gt;("X-Correlation-ID", "Correlation Id for the request");.
        /// </code>
        /// <param name="parameterName"></param>
        /// <param name="description"></param>
        /// <param name="required"></param>
        public AddHeaderOperationFilter(string parameterName, string description, bool required = false)
        {
            _parameterName = parameterName;
            _description = description;
            _required = required;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            (operation.Parameters ?? (operation.Parameters = new List<OpenApiParameter>())).Add(new OpenApiParameter
            {
                Name = _parameterName,
                In = ParameterLocation.Header,
                Description = _description,
                Required = _required,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
