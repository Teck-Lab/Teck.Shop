# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-composite AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Packages.props", "src/"]
COPY ["src/services/catalog/Directory.Build.props", "src/services/catalog/"]
COPY ["src/services/catalog/Catalog.MigrationService/Catalog.MigrationService.csproj", "src/services/catalog/Catalog.MigrationService/"]
COPY ["src/buildingblocks/TeckShop.Core/TeckShop.Core.csproj", "src/buildingblocks/TeckShop.Core/"]
COPY ["src/buildingblocks/TeckShop.Persistence/TeckShop.Persistence.csproj", "src/buildingblocks/TeckShop.Persistence/"]
COPY ["src/aspire/TeckShop.ServiceDefaults/TeckShop.ServiceDefaults.csproj", "src/aspire/TeckShop.ServiceDefaults/"]
COPY ["src/services/catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/services/catalog/Catalog.Infrastructure/"]
RUN dotnet restore "./src/services/catalog/Catalog.MigrationService/Catalog.MigrationService.csproj"
COPY . .
WORKDIR "/src/src/services/catalog/Catalog.MigrationService"
RUN dotnet build "./Catalog.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Catalog.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.MigrationService.dll"]
