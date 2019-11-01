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

FROM kdcllc/dotnet:3.0-sdk-alpine as builder
RUN dotnet publish "./src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.0-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.AspNetCore.Sample"]

