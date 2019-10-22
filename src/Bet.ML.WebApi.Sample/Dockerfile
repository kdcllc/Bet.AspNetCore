#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        Creates Docker image for AspNetCore WebApi Project.
#               1. Build the image.
#
# BUILD:        docker build -t kdcllc/bet-aspnetcore-webapi-sample:v1 -f "Dockerfile" .
#               docker build -t betregistry.azurecr.io/bet-aspnetcore-webapi-sample:v1 -f "Dockerfile" .
#
# RUN:          1. docker run -d --name bet-aspnetcore-webapi-sample kdcllc/bet-aspnetcore-webapi-sample:v1
#
#------------------------------------------------------------------------------------------------------------------------

# docker build --rm -f "src\Bet.ML.WebApi.Sample\Dockerfile" -t betregistry.azurecr.io/betmlwebapisample:latest .
FROM kdcllc/dotnet:3.0-sdk-buster as builder
RUN dotnet publish "./src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj" -c Release  -o /app

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Bet.ML.WebApi.Sample.dll"]
