#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the AspNetCore Sample.
#               - Azure Key Vault
#               - Azure Blob Storage (sql file and also data protection)
#               - Azure ApplInsights
#
#
#
# BUILD:        docker build --rm -f "src\Bet.AspNetCore.Sample\Dockerfile" -t kdcllc/bet:web .
#               --no-cache flag can be used
#
# RUN:          docker run -d -p 5000:80 kdcllc/bet:web
#
#
# PUSH:         docker push kdcllc/bet:web
#
#               kubectl exec --stdin --tty betweb -- /bin/sh
#------------------------------------------------------------------------------------------------------------------------

ARG SOLUTION_BASE=false
ARG NUGET_RESTORE=-v=q
ARG RUNTESTS=false
ARG VERBOSE=false
ARG PROJECT_PATH=/src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj

FROM kdcllc/dotnet-sdk:5.0-alpine as builder
RUN dotnet publish "./src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.AspNetCore.Sample"]

