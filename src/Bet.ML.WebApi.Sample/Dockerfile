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
