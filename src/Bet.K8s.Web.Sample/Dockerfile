#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This dockerfile build container for Bet.K8s.Web.Sample project to test:
#                   - Azure Key Vault Conifguration access
#                   - Azure Blob Storage for SQLite database storage
#                   - Azure ApplInsights
#
# NOTE:         Please run commands from root of the solution.
#
#
# BUILD:        Run these commands from root directory
#
#               docker build --rm -f "src\Bet.K8s.Web.Sample\Dockerfile" -t kdcllc/bet:k8sweb .
#               docker build --rm -f "src\Bet.K8s.Web.Sample\Dockerfile" -t betacr.azurecr.io/bet:k8sweb .
#
# PUSH:         docker push kdcllc/bet:k8sweb
#               docker push betacr.azurecr.io/bet:k8sweb
#
#------------------------------------------------------------------------------------------------------------------------

ARG SOLUTION_BASE=false
ARG NUGET_RESTORE=-v=q
ARG RUNTESTS=false
ARG VERBOSE=false
ARG PROJECT_PATH=/src/Bet.K8s.Web.Sample/Bet.K8s.Web.Sample.csproj

FROM kdcllc/dotnet-sdk:5.0-alpine as builder
RUN dotnet publish "./src/Bet.K8s.Web.Sample/Bet.K8s.Web.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true


FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.K8s.Web.Sample"]
