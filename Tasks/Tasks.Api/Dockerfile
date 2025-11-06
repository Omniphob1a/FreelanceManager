# ==========================
# 1. Build stage
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем проекты
COPY *.sln ./
COPY Tasks.Application/*.csproj Tasks.Application/
COPY Tasks.Domain/*.csproj Tasks.Domain/
COPY Tasks.Infrastructure/*.csproj Tasks.Infrastructure/
COPY Tasks.Persistence/*.csproj Tasks.Persistence/
COPY Tasks.Api/*.csproj Tasks.Api/

# Восстанавливаем зависимости
RUN dotnet restore Tasks.Api/Tasks.Api.csproj

# Копируем все исходники
COPY . .

# Публикуем проект
WORKDIR /src/Tasks.Api
RUN dotnet publish -c Release -o /app/publish

# ==========================
# 2. Runtime stage
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Копируем опубликованные файлы из build stage
COPY --from=build /app/publish .

# EXPOSE полезно для документации, Render использует runtime PORT
EXPOSE 10000

# ENTRYPOINT для Render: подставляем динамический PORT
ENTRYPOINT ["sh", "-c", "echo [DEBUG] Starting Tasks.Api on port $PORT; ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet Tasks.Api.dll"]
