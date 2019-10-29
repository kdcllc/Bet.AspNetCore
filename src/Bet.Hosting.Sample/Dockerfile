#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the sample for hosting application.
#
# BUILD:        docker build --rm -f "src\Bet.Hosting.Sample\Dockerfile" -t kdcllc/bet-hosting-sample:v1 --build-arg PROJECT_PATH="/src/Bet.Hosting.Sample/Bet.Hosting.Sample.csproj" --build-arg SOLUTION_BASE="false" .
#
#
# RUN:          docker run -d --name bet.hosting.sample kdcllc/bet-hosting-sample:v1 --runAsCronJob=false
#               docker push kdcllc/bet-hosting-sample:v1
#
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/dotnet:3.0-sdk-alpine as builder
RUN dotnet publish "./src/Bet.Hosting.Sample/Bet.Hosting.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.0-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.Hosting.Sample"]
