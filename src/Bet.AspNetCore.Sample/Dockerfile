FROM microsoft/dotnet:3.0-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:3.0-sdk-stretch AS build
WORKDIR /src
COPY ["src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj", "src/Bet.AspNetCore.Sample/"]
RUN dotnet restore "src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj"
COPY . .
WORKDIR "/src/src/Bet.AspNetCore.Sample"
RUN dotnet build "Bet.AspNetCore.Sample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Bet.AspNetCore.Sample.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Bet.AspNetCore.Sample.dll"]