# docker build --rm -f "src\Bet.ML.WebApi.Sample\Dockerfile" -t betregistry.azurecr.io/betmlwebapisample:latest .
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview9-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview9-buster AS build
WORKDIR /src

COPY ["./Directory.Build.props", "Directory.Build.props"]
COPY ["./Directory.Build.targets", "Directory.Build.targets"]
COPY ["./build/", "build/"]

COPY ["src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj", "src/Bet.ML.WebApi.Sample/"]
COPY ["src/Bet.AspNetCore.Middleware/Bet.AspNetCore.Middleware.csproj", "src/Bet.AspNetCore.Middleware/"]
COPY ["src/Bet.Extensions.ML.Spam/Bet.Extensions.ML.Spam.csproj", "src/Bet.Extensions.ML.Spam/"]
COPY ["src/Bet.Extensions.ML/Bet.Extensions.ML.csproj", "src/Bet.Extensions.ML/"]
COPY ["src/Bet.Extensions/Bet.Extensions.csproj", "src/Bet.Extensions/"]
COPY ["src/Bet.AspNetCore.HealthChecks/Bet.AspNetCore.HealthChecks.csproj", "src/Bet.AspNetCore.HealthChecks/"]
COPY ["src/Bet.AspNetCore/Bet.AspNetCore.csproj", "src/Bet.AspNetCore/"]
COPY ["src/Bet.Extensions.Options/Bet.Extensions.Options.csproj", "src/Bet.Extensions.Options/"]
COPY ["src/Bet.Extensions.ML.Sentiment/Bet.Extensions.ML.Sentiment.csproj", "src/Bet.Extensions.ML.Sentiment/"]
COPY ["src/Bet.AspNetCore.LetsEncrypt/Bet.AspNetCore.LetsEncrypt.csproj", "src/Bet.AspNetCore.LetsEncrypt/"]
COPY ["src/Bet.Extensions.Logging/Bet.Extensions.Logging.csproj", "src/Bet.Extensions.Logging/"]
COPY ["src/Bet.Extensions.Hosting/Bet.Extensions.Hosting.csproj", "src/Bet.Extensions.Hosting/"]
RUN dotnet restore "src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj"
COPY . .
WORKDIR "/src/src/Bet.ML.WebApi.Sample"
RUN dotnet build "Bet.ML.WebApi.Sample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Bet.ML.WebApi.Sample.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Bet.ML.WebApi.Sample.dll"]
