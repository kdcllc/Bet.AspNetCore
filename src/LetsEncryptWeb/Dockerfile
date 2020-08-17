#
# BUILD:  docker build --rm -f "src\LetsEncryptWeb\Dockerfile" -t kdcllc/bet:letsencrypt .
#
# PUBLISH: docker push kdcllc/bet:letsencrypt
#

FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["img/icon.png", "img/icon.png"]
COPY ["build/", "build/"]

COPY ["src/LetsEncryptWeb/LetsEncryptWeb.csproj", "src/LetsEncryptWeb/"]
COPY ["src/Bet.Extensions.AzureVault/Bet.Extensions.AzureVault.csproj", "src/Bet.Extensions.AzureVault/"]
COPY ["src/Bet.Extensions/Bet.Extensions.csproj", "src/Bet.Extensions/"]
COPY ["src/Bet.Extensions.Options/Bet.Extensions.Options.csproj", "src/Bet.Extensions.Options/"]
COPY ["src/Bet.AspNetCore.LetsEncrypt/Bet.AspNetCore.LetsEncrypt.csproj", "src/Bet.AspNetCore.LetsEncrypt/"]
COPY ["src/Bet.Extensions.LetsEncrypt/Bet.Extensions.LetsEncrypt.csproj", "src/Bet.Extensions.LetsEncrypt/"]
COPY ["src/Bet.Extensions.Logging/Bet.Extensions.Logging.csproj", "src/Bet.Extensions.Logging/"]
COPY ["src/Bet.Extensions.Hosting/Bet.Extensions.Hosting.csproj", "src/Bet.Extensions.Hosting/"]
COPY ["src/Bet.Extensions.AzureStorage/Bet.Extensions.AzureStorage.csproj", "src/Bet.Extensions.AzureStorage/"]
RUN dotnet restore "src/LetsEncryptWeb/LetsEncryptWeb.csproj"
COPY . .
WORKDIR "/src/src/LetsEncryptWeb"
RUN dotnet build "LetsEncryptWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LetsEncryptWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LetsEncryptWeb.dll"]
