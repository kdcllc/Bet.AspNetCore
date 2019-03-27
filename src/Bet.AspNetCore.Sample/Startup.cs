using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bet.AspNetCore.Sample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Bet.Extensions.ML.Prediction;
using Microsoft.ML;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Bet.AspNetCore.Sample.Models;
using System.IO;
using Microsoft.ML.Data;
using Bet.Hosting.Sample;
using System.Reflection;

namespace Bet.AspNetCore.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfigurationValidation();

            services.AddReCapture(Configuration);

            services.AddModelPredictionEngine<SentimentObservation, SentimentPrediction>("MLContent/SentimentModel.zip", "SentimentModel");
            services.AddModelPredictionEngine<SpamInput, SpamPrediction>(mlOptions =>
            {
                mlOptions.MLContext = () =>
                {
                    var mlContext = new MLContext();
                    mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelTransfomer).Assembly);
                    mlContext.Transforms.CustomMapping<LabelInput, LabelOutput>(LabelTransfomer.Transform, nameof(LabelTransfomer.Transform));

                    return mlContext;
                };

                mlOptions.CreateModel = (mlContext) =>
                {
                    using (var fileStream = File.OpenRead("MLContent/SpamModel.zip"))
                    {
                        return mlContext.Model.Load(fileStream);
                    }
                };
            },
                "SpamModel"
                );

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
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
                //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddHealthChecks()

                .AddUriHealthCheck("200_check", builder =>
                {
                    builder.Add(option =>
                    {
                        option.AddUri("https://httpstat.us/200")
                               .UseExpectedHttpCode(HttpStatusCode.OK);
                    });

                    builder.Add(option =>
                    {
                        option.AddUri("https://httpstat.us/203")
                               .UseExpectedHttpCode(HttpStatusCode.NonAuthoritativeInformation);
                    });
                })
                .AddUriHealthCheck("ms_check", uriOptions: (options) =>
                {
                    options.AddUri("https://httpstat.us/503").UseExpectedHttpCode(503);
                })
                .AddSigtermCheck("Sigterm_shutdown_check");

            services.AddMvc()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting(routes =>
            {
                routes.MapControllers();
                routes.MapRazorPages();
            });

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            // returns 200 okay
            app.UseLivenessHealthCheck();

            // returns healthy if all healthcheks return healthy
            app.UseHealthyHealthCheck();
        }
    }
}
