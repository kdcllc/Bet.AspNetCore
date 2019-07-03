using System;
using System.Threading.Tasks;

using Bet.Hosting.Sample.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md
    /// </summary>
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                    .ConfigureAppConfiguration((hostContext, config) =>
                    {
                        config.AddEnvironmentVariables();
                        config.AddJsonFile("appsettings.json", optional: true);
                        config.AddCommandLine(args);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddScoped<ModelPathService>();
                        services.AddSingleton(new MLContext());

                        services.AddSpamDetectionModelGenerator();
                        services.TryAddScoped<SpamModelGeneratorService>();

                        services.AddSentimentModelGenerator();
                        services.TryAddScoped<SentimentModelGeneratorService>();

                        services.AddTimedHostedService(
                            (token) => Task.Run(async () =>
                            {
                                Console.WriteLine("Executing...");
                                await Task.CompletedTask;
                            }),
                            options =>
                            {
                                options.Interval = TimeSpan.FromSeconds(1);

                                options.FailMode = Extensions.Hosting.Abstractions.FailMode.LogAndRetry;
                                options.RetryInterval = TimeSpan.FromSeconds(1);
                            });
                    })
                    .UseConsoleLifetime()
                    .Build();

            var hostedServices = host.Services;

            using (host)
            {
                await host.StartAsync();

                // var logger = hostedServices.GetRequiredService<ILogger<Program>>();

                //logger.LogInformation("=================== Start Building Spam Model ============================ ");
                //var spamService = hostedServices.GetRequiredService<SpamModelGeneratorService>();
                //await spamService.GenerateModel();

                //logger.LogInformation("=================== Start Building Sentiment Model ============================ ");
                //var sentimentService = hostedServices.GetRequiredService<SentimentModelGeneratorService>();
                //await sentimentService.GenerateModel();

                //await host.StopAsync();

                // Wait for the host to shutdown
                await host.WaitForShutdownAsync();
            }
        }

        //private static void BuildSentimentDetectionModel()
        //{
        //    var builder = new Extensions.ML.Sentiment.ModelBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>();
        //    builder.LoadData();

        //    var result = builder.Train();

        //    Console.WriteLine(result.ToString());

        //    var predictor = builder.MlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(builder.Model);
        //    Console.WriteLine("=============== Predictions for below data===============");

        //    ClassifySentimentText(predictor, "This is a very rude movie");
        //    ClassifySentimentText(predictor, "Hate All Of You're Work");

        //    Console.WriteLine("=================== Saving Model to Disk ============================ ");

        //    using (var fs = new FileStream(SentimentModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
        //    {
        //        builder.MlContext.Model.Save(builder.Model, builder.TrainingSchema, fs);
        //    }

        //    Console.WriteLine("======================= Creating Model Completed ================== ");
        //}

        //public static void ClassifySentimentText(PredictionEngine<SentimentIssue, SentimentPrediction> predictor, string text)
        //{
        //    var input = new SentimentIssue { Text = text };
        //    var prediction = predictor.Predict(input);
        //    Console.WriteLine("The text '{0}' is {1} Probability of being toxic: {2}", input.Text, Convert.ToBoolean(prediction.Prediction) ? "Toxic" : "Non Toxic", prediction.Probability);
        //}
    }
}

