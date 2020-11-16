#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the sample for hosting application inside of Kubernetes Cluster.
#               - demo for ML.NET building models for SentimentModel
#               - Azure Key Vault
#               - Azure Blob Storage
#               - Azure AppInsights
#
# NOTE:         Please run commands from root of the solution.
#
# BUILD:        docker build --rm -f "src\Bet.Hosting.Sample\Dockerfile" -t kdcllc/bet:hosting .
#
#
# RUN:          docker run -d --name bet.hosting.sample kdcllc/bet:hosting --runAsCronJob=false
#               docker push kdcllc/bet:hosting
#
#------------------------------------------------------------------------------------------------------------------------
ARG SOLUTION_BASE=false
ARG NUGET_RESTORE=-v=q
ARG RUNTESTS=false
ARG VERBOSE=false
ARG PROJECT_PATH=/src/Bet.Hosting.Sample/Bet.Hosting.Sample.csproj

FROM kdcllc/dotnet-sdk:5.0-alpine as builder
RUN dotnet publish  "./src/Bet.Hosting.Sample/Bet.Hosting.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.Hosting.Sample"]
