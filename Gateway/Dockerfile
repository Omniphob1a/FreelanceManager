# syntax=docker/dockerfile:1.4
# ---------- stage: build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG CONFIG=Release

# копируем решение и csproj (только то, что нужно для restore) — это даёт кэш
COPY Gateway.sln ./
COPY Gateway/*.csproj Gateway/

# restore по конкретному проекту (можно restore по sln, но указание csproj быстрее для точности)
RUN dotnet restore Gateway/Gateway.csproj

# копируем весь исходник Gateway (и другие файлы решения при необходимости)
COPY Gateway/ Gateway/

# publish
WORKDIR /src/Gateway
RUN dotnet publish -c $CONFIG -o /app/publish Gateway.csproj

# ---------- stage: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]
