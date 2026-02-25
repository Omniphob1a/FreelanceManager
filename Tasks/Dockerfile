# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG CONFIG=Release

COPY *.sln ./

COPY Tasks.Application/*.csproj Tasks.Application/
COPY Tasks.Domain/*.csproj Tasks.Domain/
COPY Tasks.Infrastructure/*.csproj Tasks.Infrastructure/
COPY Tasks.Persistence/*.csproj Tasks.Persistence/
COPY Tasks.Api/*.csproj Tasks.Api/

# CA certs to avoid SSL errors
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*

RUN dotnet restore Tasks.Api/Tasks.Api.csproj

COPY . .

WORKDIR /src/Tasks.Api
RUN dotnet publish Tasks.Api.csproj -c $CONFIG -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasks.Api.dll"]
