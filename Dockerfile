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
