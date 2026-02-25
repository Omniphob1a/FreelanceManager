# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG CONFIG=Release

COPY *.sln ./

COPY Users.Application/*.csproj Users.Application/
COPY Users.Domain/*.csproj Users.Domain/
COPY Users.Infrastructure/*.csproj Users.Infrastructure/
COPY Users.Shared/*.csproj Users.Shared/
COPY Users.Api/*.csproj Users.Api/

# install CA certs to reduce SSL restore issues
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*

# restore only target api project
RUN dotnet restore Users.Api/Users.Api.csproj

COPY . .

WORKDIR /src/Users.Api
RUN dotnet publish Users.Api.csproj -c $CONFIG -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]
