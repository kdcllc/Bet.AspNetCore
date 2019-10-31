FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["src/AppAuthentication/AppAuthentication.csproj", "src/AppAuthentication/"]
COPY ["src/Bet.Extensions.Logging/Bet.Extensions.Logging.csproj", "src/Bet.Extensions.Logging/"]
COPY ["src/Bet.Extensions/Bet.Extensions.csproj", "src/Bet.Extensions/"]
COPY ["src/Bet.AspNetCore/Bet.AspNetCore.csproj", "src/Bet.AspNetCore/"]
COPY ["src/Bet.Extensions.Options/Bet.Extensions.Options.csproj", "src/Bet.Extensions.Options/"]
RUN dotnet restore "src/AppAuthentication/AppAuthentication.csproj"
COPY . .
WORKDIR "/src/src/AppAuthentication"
RUN dotnet build "AppAuthentication.csproj" -c Release -f netcoreapp2.2 -o /app/build

FROM build AS publish
RUN dotnet publish "AppAuthentication.csproj" -c Release -f netcoreapp2.2 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppAuthentication.dll"]
CMD ["run", "--verbose:debug"]

