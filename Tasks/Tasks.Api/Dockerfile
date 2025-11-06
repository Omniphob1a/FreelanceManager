# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем проекты
COPY *.sln ./
COPY Tasks.Application/*.csproj Tasks.Application/
COPY Tasks.Domain/*.csproj Tasks.Domain/
COPY Tasks.Infrastructure/*.csproj Tasks.Infrastructure/
COPY Tasks.Persistence/*.csproj Tasks.Persistence/
COPY Tasks.Api/*.csproj Tasks.Api/

RUN dotnet restore Tasks.Api/Tasks.Api.csproj

COPY . .

WORKDIR /src/Tasks.Api
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV PORT=10001
EXPOSE $PORT

# ENTRYPOINT запускает миграции и сервис
ENTRYPOINT ["dotnet", "Tasks.Api.dll"]
