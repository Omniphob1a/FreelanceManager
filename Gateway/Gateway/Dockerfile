# ---------- stage 1 : build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем решение и csproj‑файл ‑ путь берётся из контекста ./Gateway
COPY Gateway.sln ./
COPY Gateway/*.csproj Gateway/

# restore
RUN dotnet restore Gateway/Gateway.csproj

# копируем весь исходный код Gateway
COPY Gateway/ Gateway/

# publish
WORKDIR /src/Gateway
RUN dotnet publish -c Release -o /app/publish

# ---------- stage 2 : runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Gateway.dll"]
