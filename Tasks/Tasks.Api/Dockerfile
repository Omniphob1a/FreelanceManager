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

# Устанавливаем корневые сертификаты, чтобы путь /etc/ssl/certs/ca-certificates.crt существовал
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates \
 && rm -rf /var/lib/apt/lists/*

# Копируем опубликованные файлы из build stage
COPY --from=build /app/publish .

# Документируем порт (в compose вы пробрасываете 80)
EXPOSE 80

ENTRYPOINT ["dotnet", "Tasks.Api.dll"]