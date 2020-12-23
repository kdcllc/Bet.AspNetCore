using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Bet.AspNetCore.Swagger.OperationFilters;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenWithApiVersion<T>(
            this IServiceCollection services,
            bool includeXmlComments = true)
        {
            var fileName = typeof(T).GetTypeInfo().Assembly.GetName().Name;
            return services.AddSwaggerGenWithApiVersion(fileName, includeXmlComments);
        }

        /// <summary>
        /// Add Swagger Generation with UI for Api Versions support.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="appName">The name of the application.</param>
        /// <param name="includeXmlComments">The flag to include auto-generated xml dll comments. The Default is true.</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerGenWithApiVersion(
        this IServiceCollection services,
        string? appName = null,
        bool includeXmlComments = true)
        {
            services.AddVersionedApiExplorer();
            services.AddOptions<ApiExplorerOptions>()
                    .Configure(options =>
                    {
                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route templates
                        options.SubstituteApiVersionInUrl = true;
                    });

            services.AddApiVersioning(options =>
            {
                // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddOptions<SwaggerUIOptions>()
                    .Configure<IApiVersionDescriptionProvider>((options, provider) =>
                    {
                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerEndpoint(
                                 $"/swagger/{description.GroupName}/swagger.json",
                                 description.GroupName.ToUpperInvariant());
                        }
                    });

            services.AddOptions<SwaggerGenOptions>()
                    .Configure<IServiceProvider>((options, sp) =>
                    {
                        options.EnableAnnotations();

                        var config = sp.GetRequiredService<IConfiguration>();

                        var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
                        var appliationName = appName ?? config[WebHostDefaults.ApplicationKey];

                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerDoc(
                                description.GroupName,
                                CreateInfoForApiVersion(description, appliationName));
                        }

                        if (includeXmlComments)
                        {
                            options.IncludeXmlComments(GetXmlDocPath(appliationName));
                        }

                        options.OperationFilter<SwaggerDefaultValues>();

                        // add a custom operation filter to support header input
                        // options.OperationFilter<AddHeaderOperationFilter>("traceidparent", "Trace Id in w3c format. Ex: 00-95a8affeb7d85b4fb08fe28f19ba815e-6003203a14071244-00 <br/>https://w3c.github.io/trace-context/#examples-of-http-traceparent-headers");

                        options.AddBearer();

                        // https://github.com/domaindrivendev/Swashbuckle/issues/142
                        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                    });

            services.AddSwaggerGen();

            return services;
        }

        private static string GetXmlDocPath(string appName)
        {
            if (appName.Contains(','))
            {
                // if app name is the full assembly name, just grab the short name part
                appName = appName.Substring(0, appName.IndexOf(','));
            }

            return Path.Combine(AppContext.BaseDirectory, $"{appName}.xml");
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description, string appliationName)
        {
            var info = new OpenApiInfo
            {
                Title = $"{appliationName} API {description.ApiVersion}",
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
