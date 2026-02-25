# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG CONFIG=Release

# copy solution and project files needed for restore (only csproj files)
COPY *.sln ./

# копируем csproj'ы всех проектах, на которые может ссылаться API
COPY Projects.Application/*.csproj Projects.Application/
COPY Projects.Domain/*.csproj Projects.Domain/
COPY Projects.Infrastructure/*.csproj Projects.Infrastructure/
COPY Projects.Persistence/*.csproj Projects.Persistence/
COPY Projects.Shared/*.csproj Projects.Shared/
COPY Projects.Api/*.csproj Projects.Api/

# install CA certs to avoid SSL issues during restore
RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates && \
    update-ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Restore **конкретного** проекта (не всего sln) — это избегает ошибок с тестовыми проектами
RUN dotnet restore Projects.Api/Projects.Api.csproj

# copy remaining source
COPY . .

# publish
WORKDIR /src/Projects.Api
RUN dotnet publish Projects.Api.csproj -c $CONFIG -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Projects.Api.dll"]
