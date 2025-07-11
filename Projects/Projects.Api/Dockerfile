FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем решение и проекты из корня контекста (Projects/)
COPY *.sln ./
COPY Projects.Application/*.csproj Projects.Application/
COPY Projects.Domain/*.csproj Projects.Domain/
COPY Projects.Infrastructure/*.csproj Projects.Infrastructure/
COPY Projects.Persistence/*.csproj Projects.Persistence/
COPY Projects.Shared/*.csproj Projects.Shared/
COPY Projects.Api/*.csproj Projects.Api/

RUN dotnet restore Projects.Api/Projects.Api.csproj

COPY . .

WORKDIR /src/Projects.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Projects.Api.dll"]
