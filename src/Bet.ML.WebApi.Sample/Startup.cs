using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bet.AspNetCore.Middleware.Diagnostics;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam;
using Bet.Extensions.ML.Spam.Models;
using Bet.ML.WebApi.Sample.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;
using Microsoft.OpenApi.Models;

namespace Bet.ML.WebApi.Sample
{
    public class Startup
    {
        private static readonly string AppName = "Bet.ML.WebApi.Sample";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDeveloperListRegisteredServices(o =>
            {
                o.PathOutputOptions = PathOutputOptions.Json;
            });

            services.AddControllers();

            services.AddHealthChecks().AddSigtermCheck("Sigterm_shutdown_check");

            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo { Title = $"{AppName} API", Version = "v1" }));

            // add ML.NET Models
            services.AddSpamDetectionModelBuilder();
            services.AddSentimentModelBuilder();

            // override the storage provider
            services.AddSingleton<IModelStorageProvider, InMemoryModelStorageProvider>();

            services.AddModelPredictionEngine<SpamInput, SpamPrediction>(mlOptions =>
            {
                mlOptions.CreateModel = (mlContext) =>
                {
                    var storage = mlOptions.ServiceProvider.GetRequiredService<IModelStorageProvider>();


                    var model = GetModel();

                    ChangeToken.OnChange(() => storage.GetReloadToken(), () => model = GetModel());

                    return model;

                    ITransformer GetModel()
                    {
                        var model = storage.LoadModelAsync(nameof(SpamModelBuilderService), CancellationToken.None).GetAwaiter().GetResult();
                        return mlContext.Model.Load(model, out var inputSchema);
                    }
                };
            }, "SpamModel");

            services.AddModelPredictionEngine<SentimentIssue, SentimentPrediction>(mlOptions =>
            {
                mlOptions.CreateModel = (mlContext) =>
                {
                    var storage = mlOptions.ServiceProvider.GetRequiredService<IModelStorageProvider>();

                    var model = GetModel();

                    ChangeToken.OnChange(() => storage.GetReloadToken(), () => model = GetModel()) ;

                    return model;

                    ITransformer GetModel()
                    {
                        var model = storage.LoadModelAsync(nameof(SentimentModelBuilderService), CancellationToken.None).GetAwaiter().GetResult();
                        return mlContext.Model.Load(model, out var inputSchema);
                    }

                };
            }, "SentimentModel");

            services.AddScheduler(builder =>
            {
                builder.AddJob<RebuildMLModelScheduledJob, RebuildMLModelsOptions>();
                builder.UnobservedTaskExceptionHandler = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // returns 200 okay
            app.UseLivenessHealthCheck();
            // returns healthy if all healthcheks return healthy
            app.UseHealthyHealthCheck();

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{AppName} API v1"));
        }
    }
}
