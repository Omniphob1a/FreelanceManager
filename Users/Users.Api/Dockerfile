FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Users.Application/*.csproj Users.Application/
COPY Users.Domain/*.csproj Users.Domain/
COPY Users.Infrastructure/*.csproj Users.Infrastructure/
COPY Users.Api/*.csproj Users.Api/

RUN dotnet restore Users.Api/Users.Api.csproj

COPY . .

WORKDIR /src/Users.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]
