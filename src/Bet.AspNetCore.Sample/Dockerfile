#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        This Dockerfile builds the AspNetCore Sample.
#
# BUILD:        docker build --rm -f "src\Bet.AspNetCore.Sample\Dockerfile" -t kdcllc/bet:web --build-arg PROJECT_PATH="/src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" --build-arg SOLUTION_BASE="false" .
#
#
# RUN:          docker run -d -p 5000:80 kdcllc/bet:web
#               docker push kdcllc/bet:web
#
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/bet:base as builder
RUN dotnet publish "./src/Bet.AspNetCore.Sample/Bet.AspNetCore.Sample.csproj" -r linux-musl-x64 -o out --self-contained true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
WORKDIR /app

COPY --from=builder /app/out ./
ENTRYPOINT ["./Bet.AspNetCore.Sample"]

