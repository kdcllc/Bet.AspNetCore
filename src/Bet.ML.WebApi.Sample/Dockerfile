#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        Creates Docker image for AspNetCore WebApi Project.
#
# BUILD:        docker build --rm -f "src\Bet.ML.WebApi.Sample\Dockerfile" -t kdcllc/bet:webapi --build-arg PROJECT_PATH="/src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj" --build-arg SOLUTION_BASE="false" .
#
# RUN:          docker run -d -p 4000:80 kdcllc/bet:webapi
#               docker push kdcllc/bet:webapi
#
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/bet:base as builder
RUN dotnet publish "./src/Bet.ML.WebApi.Sample/Bet.ML.WebApi.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
RUN apk update && apk upgrade && \
    apk add --no-cache -U wget curl bash

WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.ML.WebApi.Sample"]
