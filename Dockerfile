#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        Creates Docker image for building and packing the Nuget Packages.
#               1. Build the image
#               2. Run the container to get the packages out of the image.
#
# BUILD:        docker build -t kdcllc/betaspnetcore.nuget:v1 -f "Dockerfile" --build-arg VERSION=1.4.5-preview .
#
# RUN:          1. docker run -d --name betaspnetcore.nuget kdcllc/betaspnetcore.nuget:v1
#               2. docker cp betaspnetcore.nuget:/app/nugets ${PWD}/packages
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/dotnet:3.0-sdk-base-buster as builder
ARG VERSION
WORKDIR /app

COPY ./*.sln ./*.props ./*.targets ./
COPY ./build/* ./build/

COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
COPY test/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p test/${file%.*}/ && mv $file test/${file%.*}/; done

RUN dotnet restore Bet.AspNetCore.sln
COPY ./src ./src
COPY ./test ./test
COPY .git ./.git

RUN dotnet build Bet.AspNetCore.sln -c Release /p:Version=${VERSION}
RUN dotnet pack Bet.AspNetCore.sln -c Release --no-build -o /app/nugets
