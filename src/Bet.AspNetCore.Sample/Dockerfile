#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the AspNetCore Sample.
#
# BUILD:        docker build --rm -f "src\Bet.AspNetCore.Sample\Dockerfile" -t kdcllc/bet-aspnetcore-web-sample:v1 --build-arg PROJECT_PATH="/src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" --build-arg SOLUTION_BASE="false" .
#
#
# RUN:          docker run -d --name bet.aspnetcore.web.sample kdcllc/bet-aspnetcore-web-sample:v1 -p 5000:80 -e "ASPNETCORE_URLS=http://+:80"
#               docker push kdcllc/kdcllc/bet-aspnetcore-web-sample:v1
#
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/dotnet:3.0-sdk-buster as builder
RUN dotnet publish "./src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" -c Release  -o /app

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=builder /app .
ENTRYPOINT ["dotnet", "Bet.AspNetCore.Sample.dll"]
