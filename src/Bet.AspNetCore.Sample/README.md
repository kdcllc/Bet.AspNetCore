# AspNetCore Web Application Sample

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Summary

This project demonstrates how to utilize:

- `Sentiment` and `Spam` ML models with AspNetCore application

1. ML.NET Batch predictions requests.
2. ML.NET Models Generations on start.
3. ML.NET Models generation on schedule basis.
4. ML.NET Models HealthChecks
5. ML.NET Models file watch with File, Azure providers

- Kubernetes `DataProtection` provider to store the encryptions keys in Azure Storage Blob.

The building of the models occurs on the launch of the application and the Http traffic is not served until the Initial job has been completed.

[Live https://betweb.kingdavidconsulting.com/](https://betweb.kingdavidconsulting.com/)

These models can be found at the following projects:

- [`Bet.Extensions.ML.Sentiment` Library](../../src/Bet.Extensions.ML.Sentiment/)
- [`Bet.Extensions.ML.Spam` Library](../../src/Bet.Extensions.ML.Spam/)

The folder named `MLContent` contains pre-generated ML.NET models that the Web Api Controllers use for the predictions when `true` is set to `true`.

## Build and Deploy

Testing K8 Cron Job in the local cluster please follow the setup instruction per [K8.DotNetCore.Workshop](https://github.com/kdcllc/K8.DotNetCore.Workshop).

Make sure to execute all of the commands from the solution folder.

1. Build the Image

```bash
    # builds and runs the container
    docker-compose -f "docker-compose.yml" -f "docker-compose.override.yml" up -d  bet.web

    # simply builds the image
    docker-compose -f "docker-compose.yml" up -d --build --no-recreate  bet.web

    # publish if needed
    docker push kdcllc/bet:web
```

2. Helm Install

```bash

    # install web api in the local Kubernetes cluster
    helm install betweb k8s/betweb --set ingress.enabled=false,aadpodidbinding=test,local.enabled=true

    # unistall web api project
    helm uninstall betweb

    # verify the pod
    kubectl describe pod betweb
```

## ML Model HealthCheck

In a situation where the model is being build in the same container and the web api if the model generation fails the container should be restarted.

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

## Machine Learning

Batch testing of values

```json
[
  {
    "label": false,
    "text": "This is a very rude movie"
  },
  {
    "label": true,
    "text": "Hate All Of You're Work"
  }
]
```

## Future work

- To enable the functionality to accept new data point and storing them inside of SQLite or other storage.

## About Docker Images

This repo is utilizing [King David Consulting LLC Docker Images](https://hub.docker.com/u/kdcllc):

- [kdcllc/dotnet-sdk:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk-vscode):  - the docker image for templated `DotNetCore` build of the sample web application.

- [kdcllc/dotnet-sdk-vscode:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk/tags): the docker image for the Visual Studio Code In container development.

- [Docker Github repo](https://github.com/kdcllc/docker/blob/master/dotnet/dotnet-docker.md)
