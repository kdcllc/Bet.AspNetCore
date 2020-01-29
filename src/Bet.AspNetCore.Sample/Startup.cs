using System;
using System.Collections.Generic;

using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.AspNetCore.Sample.Data;
using Bet.AspNetCore.Sample.Models;
using Bet.AspNetCore.Sample.Options;
using Bet.Extensions.ML.Azure.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Serilog;

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
            // enables custom options validations on bind and configure.
            services.AddConfigurationValidation();

            services.AddDataProtectionAzureStorage(Configuration);

            services.AddAppInsightsTelemetry();

            services.AddDeveloperListRegisteredServices(o =>
            {
                o.PathOutputOptions = PathOutputOptions.Json;
            });

            services.AddReCapture(Configuration);

            // add sentiment model
            services.AddModelPredictionEngine<SentimentObservation, SentimentPrediction>(MLModels.SentimentModel)
                .From<SentimentObservation, SentimentPrediction, FileModelLoader>(options =>
                {
                    options.ModelFileName = "MLContent/SentimentModel.zip";
                });

            // add spam model
            //services.AddModelPredictionEngine<SpamInput, SpamPrediction>(
            //    MLModels.SpamModel)
            //    .From<SpamInput, SpamPrediction, FileModelLoader>(
            //    options =>
            //    {
            //        options.ModelFileName = "MLContent/SpamModel.zip";
            //    });

            services.AddAzureStorageAccount(MLModels.SpamModel).AddAzureBlobContainer(MLModels.SpamModel, "models");
            services.AddModelPredictionEngine<SpamInput, SpamPrediction>(MLModels.SpamModel)
                    .From<SpamInput, SpamPrediction, AzureStorageModelLoader>(options =>
                    {
                        options.WatchForChanges = true;
                        options.ReloadInterval = TimeSpan.FromSeconds(40);
                    });

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
                .AddMachineLearningModelCheck<SentimentObservation, SentimentPrediction>(
                $"{MLModels.SentimentModel}_check",
                options =>
                {
                    options.ModelName = MLModels.SentimentModel;
                    options.SampleData = new SentimentObservation
                    {
                        SentimentText = "This is a very rude movie"
                    };
                })
                .AddAzureBlobStorageCheck("blob_check", "files", options =>
                {
                    options.Name = "betstorage";
                })
                .AddAzureQueuetorageCheck("queue_check", "betqueue")
                .AddSigtermCheck("sigterm_check")
                .AddLoggerPublisher(new List<string> { "sigterm_check" });

            services.AddMvc().AddNewtonsoftJson();

            services.AddRazorPages().AddNewtonsoftJson();

            services.AddAzureStorageAccount()
                .AddAzureBlobContainer<UploadsBlobOptions>()
                .AddAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();

            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo { Title = $"{AppName} API", Version = "v1" }));

            // Preview 8 has been fixed https://github.com/microsoft/aspnet-api-versioning/issues/499
            services.AddSwaggerGenWithApiVersion();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
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

            app.UseOrNotHttpsRedirection();

            app.UseStaticFiles();
            app.UseSerilogRequestLogging();

            app.UseAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI();

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
