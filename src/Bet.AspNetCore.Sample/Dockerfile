FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview9-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview9-buster AS build

WORKDIR /src
COPY ["./Directory.Build.props", "Directory.Build.props"]
COPY ["./Directory.Build.targets", "Directory.Build.targets"]
COPY ["./build/", "build/"]
COPY ["src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj", "src/Bet.AspNetCore.Sample/"]
COPY ["src/Bet.AspNetCore.Middleware/Bet.AspNetCore.Middleware.csproj", "src/Bet.AspNetCore.Middleware/"]
COPY ["src/Bet.Extensions.Logging/Bet.Extensions.Logging.csproj", "src/Bet.Extensions.Logging/"]
COPY ["src/Bet.Extensions.ML.Spam/Bet.Extensions.ML.Spam.csproj", "src/Bet.Extensions.ML.Spam/"]
COPY ["src/Bet.Extensions.ML/Bet.Extensions.ML.csproj", "src/Bet.Extensions.ML/"]
COPY ["src/Bet.Extensions/Bet.Extensions.csproj", "src/Bet.Extensions/"]
COPY ["src/Bet.AspNetCore.HealthChecks/Bet.AspNetCore.HealthChecks.csproj", "src/Bet.AspNetCore.HealthChecks/"]
COPY ["src/Bet.AspNetCore/Bet.AspNetCore.csproj", "src/Bet.AspNetCore/"]
COPY ["src/Bet.Extensions.Options/Bet.Extensions.Options.csproj", "src/Bet.Extensions.Options/"]
COPY ["src/Bet.Extensions.AzureStorage/Bet.Extensions.AzureStorage.csproj", "src/Bet.Extensions.AzureStorage/"]
COPY ["src/Bet.Extensions.AzureVault/Bet.Extensions.AzureVault.csproj", "src/Bet.Extensions.AzureVault/"]
COPY ["src/Bet.AspNetCore.ReCapture/Bet.AspNetCore.ReCapture.csproj", "src/Bet.AspNetCore.ReCapture/"]
RUN dotnet restore "src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj"
COPY . .
WORKDIR "/src/src/Bet.AspNetCore.Sample"
RUN dotnet build "Bet.AspNetCore.Sample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Bet.AspNetCore.Sample.csproj" -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Bet.AspNetCore.Sample.dll"]
