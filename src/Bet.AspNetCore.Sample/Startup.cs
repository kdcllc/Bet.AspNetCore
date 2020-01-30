using System;

using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.AspNetCore.Sample.Data;
using Bet.AspNetCore.Sample.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            // configure Options for the App example.
            services.ConfigureWithDataAnnotationsValidation<AppSetting>(Configuration, "App");

            // requires AzureDataProtection=true to register
            services.AddDataProtectionAzureStorage(Configuration);

            services.AddAppInsightsTelemetry();

            services.AddDeveloperListRegisteredServices(o =>
            {
                o.PathOutputOptions = PathOutputOptions.Json;
            });

            services.AddReCapture(Configuration);

            // adds spam model and monitors for changes.
            services.AddSpamModelPrediction(Configuration);

            // adds sentiment model and monitors for changes.
            services.AddSentimentModelPrediction(Configuration);

            // adds healthchecks
            services.AddAppHealthChecks(Configuration);

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

            services.AddMvc()
                    .AddNewtonsoftJson();

            services.AddRazorPages()
                    .AddNewtonsoftJson();

            services.AddAzureStorageAccount()
                .AddAzureBlobContainer<UploadsBlobOptions>()
                .AddAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();

            // Preview 8 has been fixed https://github.com/microsoft/aspnet-api-versioning/issues/499
            services.AddSwaggerGenWithApiVersion(AppName);

            var buildModels = Configuration.GetValue<bool>("BuildModels");

            if (buildModels)
            {
                services.AddScheduler(builder =>
                {
                    builder.AddJob<ModelBuilderJob, ModelBuilderOptions>();
                    builder.UnobservedTaskExceptionHandler = null;
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
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

            // demonstrates static file cache
            app.UseStaticFilesWithCache(TimeSpan.FromSeconds(10));

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
