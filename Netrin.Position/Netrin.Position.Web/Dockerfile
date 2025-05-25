FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["Netrin.Position.sln", ""]

COPY ["Netrin.Position.Web/Netrin.Position.Web.csproj", "Netrin.Position.Web/"]
COPY ["Netrin.Position.Application/Netrin.Position.Application.csproj", "Netrin.Position.Application/"]
COPY ["Netrin.Position.Domain/Netrin.Position.Domain.csproj", "Netrin.Position.Domain/"]
COPY ["Netrin.Position.Adapter.MySql/Netrin.Position.Adapter.MySql.csproj", "Netrin.Position.Adapter.MySql/"]
COPY ["Netrin.Position.Infra.MySql/Netrin.Position.Infra.MySql.csproj", "Netrin.Position.Infra.MySql/"]
COPY ["Netrin.Position.Infra.Packages/Netrin.Position.Infra.Packages.csproj", "Netrin.Position.Infra.Packages/"]
COPY ["Netrin.Position.Back.Tests/Netrin.Position.Back.Tests.csproj", "Netrin.Position.Back.Tests/"]

RUN dotnet restore -s https://api.nuget.org/v3/index.json

COPY . .

WORKDIR /app/Netrin.Position.Web

RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false /p:DisableImplicitNuGetFallbackFolder=true -s https://api.nuget.org/v3/index.json

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

COPY --from=build /app/Netrin.Position.Web/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Netrin.Position.Web.dll"]