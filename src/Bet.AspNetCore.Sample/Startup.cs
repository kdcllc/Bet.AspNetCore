using System.Collections.Generic;
using System.IO;

using Bet.AspNetCore.Logging.Azure;
using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.AspNetCore.Sample.Data;
using Bet.AspNetCore.Sample.Models;
using Bet.AspNetCore.Sample.Options;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Bet.AspNetCore.Sample
{
    public class Startup
    {
        private const string AppName = "Bet.AspNetCore.Sample";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigurationValidation();

            var enabledDataProtection = Configuration.GetValue<bool>("EnabledDataProtection");
            if (enabledDataProtection)
            {
                services.AddDataProtectionAzureStorage();
            }

            var instrumentId = Configuration.Bind<ApplicationInsightsOptions>("ApplicationInsights", true);

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.InstrumentationKey = instrumentId.InstrumentationKey;
            });

            services.AddDeveloperListRegisteredServices(o =>
            {
                o.PathOutputOptions = PathOutputOptions.Json;
            });

            services.AddReCapture(Configuration);

            services.AddModelPredictionEngine<SentimentObservation, SentimentPrediction>("MLContent/SentimentModel.zip", "SentimentModel");

            services.AddModelPredictionEngine<SpamInput, SpamPrediction>(
                mlOptions =>
            {
                mlOptions.CreateModel = (mlContext) =>
                {
                    using (var fileStream = File.OpenRead("MLContent/SpamModel.zip"))
                    {
                        return mlContext.Model.Load(fileStream, out var inputSchema);
                    }
                };
            }, "SpamModel");

            // configure Options for the App.
            services.ConfigureWithDataAnnotationsValidation<AppSetting>(Configuration, "App");

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var dbPath = Configuration.GetValue<string>("DatabasePath");
                var connectionString = $"Filename={dbPath}app.db";
                options.UseSqlite(connectionString);

                // options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddHealthChecks()
                .AddUriHealthCheck("ms_check", uriOptions: (options) =>
                {
                    options.AddUri("https://httpstat.us/503").UseExpectedHttpCode(503);
                })
                .AddMachineLearningModelCheck<SentimentObservation, SentimentPrediction>("Sentiment_Check")
                .AddAzureBlobStorageCheck("files_check", "files", options =>
                {
                    options.Name = "betstorage";
                })
                .AddSigtermCheck("sigterm_check")
                .AddLoggerPublisher(new List<string> { "sigterm_check" });

            services.AddMvc().AddNewtonsoftJson();

            services.AddRazorPages().AddNewtonsoftJson();

            services.AddStorageBlob()
                .AddBlobContainer<UploadsBlobOptions>();

            services.AddAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();

            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo { Title = $"{AppName} API", Version = "v1" }));

            // Preview 8 has been fixed https://github.com/microsoft/aspnet-api-versioning/issues/499
            services.AddSwaggerGenWithApiVersion();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider provider,
            IConfiguration configuration)
        {
            app.UseIfElse(
                env.IsDevelopment(),
                dev =>
                {
                    app.UseDeveloperExceptionPage();
                    app.UseDatabaseErrorPage();
                    app.UseDeveloperListRegisteredServices();
                    return dev;
                },
                prod =>
                {
                    app.UseExceptionHandler("/Error");

                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();

                    return prod;
                });

            var enableHttpsRedirection = configuration.GetValue<bool>("EnabledHttpsRedirection");

            if (enableHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            // Preview 8 has been fixed https://github.com/microsoft/aspnet-api-versioning/issues/499
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                         $"/swagger/{description.GroupName}/swagger.json",
                         description.GroupName.ToUpperInvariant());
                }
            });

            // https://devblogs.microsoft.com/aspnet/blazor-now-in-official-preview/
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();

                // returns 200 okay
                endpoints.MapLivenessHealthCheck();
                endpoints.MapHealthyHealthCheck();
            });
        }
    }
}
