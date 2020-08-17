#------------------------------------------------------------------------------------------------------------------------
#
# USAGE:        Creates Docker image for building and packing the Nuget Packages.
#               1. Build the image
#               2. Run the container to get the packages out of the image.
#
# BUILD:        docker build -t kdcllc/bet:nuget-build -f "Dockerfile" --build-arg VERSION=2.1.1-preview --build-arg NUGET_RESTORE="-v=m" .
#
# RUN:          1. docker run -d --name bet.nuget.build kdcllc/bet:nuget-build
#               2. docker cp bet.nuget.build:/app/nugets ${PWD}/packages
#------------------------------------------------------------------------------------------------------------------------

FROM kdcllc/dotnet-sdk:5.0-focal as builder

RUN apt-get -y update &&\
    apt-get -y install git &&\
    rm -rf /var/lib/apt/lists/*

ARG VERSION
WORKDIR /app

COPY ./img ./img
COPY .git ./.git

RUN dotnet pack Bet.AspNetCore.sln -c Release -p:Version=${VERSION} --no-build -o /app/nugets
