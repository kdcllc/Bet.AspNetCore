# AspNetCore WebApi Machine Learning Example for `Sentiment` and `Spam` Ml model predictions

This project demonstrates how to utilize `Sentiment` and `Spam` ML models with AspNetCore application.

The building of the models occurs on the launch of the application and the Http traffic is not served until the Initial job has been completed.

[Live](https://betweb.kingdavidconsulting.com/)

These models can be found at the following projects:

- [`Bet.Extensions.ML.Sentiment` Library](../../src/Bet.Extensions.ML.Sentiment/README.md)
- [`Bet.Extensions.ML.Spam` Library](../../src/Bet.Extensions.ML.Spam/README.md)

The folder named `Data` contains pre-generated ML.NET models that the Api Controller uses for the predictions.

## Build and Deploy

Testing K8 Cron Job in the local cluster please follow the setup instruction per [K8.DotNetCore.Workshop](https://github.com/kdcllc/K8.DotNetCore.Workshop).

Make sure to execute all of the commands from the solution folder.

1. Build the Image

```bash
    # builds and runs the container
    docker-compose -f "docker-compose.yml" -f "docker-compose.override.yml" up -d  bet.aspnetcore.web

    # simply builds the image
    docker-compose -f "docker-compose.yml" up -d --build --no-recreate  bet.aspnetcore.web

    # publish if needed
    docker push kdcllc/bet-aspnetcore-web-sample:v1
```

2. Helm Install

```bash

    # install web api in the local Kubernetes cluster
     helm install bet-web-sample --set service.port=5000 -n betwebsample

    # delete web api
    helm delete  betwebsample --purge
```

```csharp

            services.AddHealthChecks()
                 .AddSslCertificateCheck("kdcllc", "https://kingdavidconsulting.com")
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
                .AddMachineLearningModelCheck<SpamInput, SpamPrediction>("Spam_Check")
                .AddMachineLearningModelCheck<SentimentObservation, SentimentPrediction>("Sentiment_Check")
                .AddAzureBlobStorageCheck("files_check", "files", options =>
                {
                    options.Name = "betstorage";
                })
                .AddSigtermCheck("sigterm_check")
                .AddLoggerPublisher(new List<string> { "sigterm_check" });
```


## ML Model HealthCheck

In a situation where the model is being build in the same container and the web api if the model generation fails the container should be restarted.

## Future work

- To enable the functionality to accept new data point and storing them inside of SQLite or other storage.
- To have a schedule job that rebuilds and verifies the models before making them available to predictive engine.
- Off load the initial model generation to a pre-generated model so the app startup in the pod. It would require adding model loaders func into the `Bet.Extensions.ML`.
