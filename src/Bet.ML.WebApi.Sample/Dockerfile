#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        Creates Docker image for AspNetCore WebApi Project.
#               1. Build the image.
#
# BUILD:        docker build --rm -f "src\Bet.ML.WebApi.Sample\Dockerfile" -t kdcllc/bet-aspnetcore-webapi-sample:v1 --build-arg PROJECT_PATH="/src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj" --build-arg SOLUTION_BASE="false" .
#
#
# RUN:          docker run -d --name bet.aspnetcore.webapi.sample kdcllc/bet-aspnetcore-webapi-sample:v1 -p 4000:80 -e "ASPNETCORE_URLS=http://+:80"
#               docker push kdcllc/bet-aspnetcore-webapi-sample:v1
#
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/dotnet-sdk:3.1-alpine as builder
RUN dotnet publish "./src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.ML.WebApi.Sample"]
