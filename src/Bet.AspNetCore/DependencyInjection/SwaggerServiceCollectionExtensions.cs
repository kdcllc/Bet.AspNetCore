using System;
using System.IO;
using System.Linq;

using Bet.AspNetCore.Swagger.OperationFilters;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
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

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddMvcCore().AddApiExplorer()
                           .AddAuthorization()
                           .AddFormatterMappings()
                           .AddCacheTagHelper()
                           .AddDataAnnotations();

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
                        var config = sp.GetRequiredService<IConfiguration>();

                        var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
                        var appliationName = appName ?? config[WebHostDefaults.ApplicationKey];

                        foreach (var description in provider.ApiVersionDescriptions)
                        {
                            options.SwaggerDoc(
                                description.GroupName,
                                new OpenApiInfo
                                {
                                    Title = $"{appliationName} API {description.ApiVersion}",
                                    Version = description.ApiVersion.ToString()
                                });
                        }

                        if (includeXmlComments)
                        {
                            options.IncludeXmlComments(GetXmlDocPath(appliationName));
                        }

                        options.OperationFilter<SwaggerDefaultValues>();

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

            return Path.Combine(AppContext.BaseDirectory, appName + ".xml");
        }
    }
}
