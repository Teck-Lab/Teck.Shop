# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-composite AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/services/catalog/Directory.Build.props", "src/services/catalog/"]
COPY ["src/Directory.Build.props", "src/"]
COPY ["Directory.Build.props", "."]
COPY ["src/services/catalog/Catalog.Api/Catalog.Api.csproj", "src/services/catalog/Catalog.Api/"]
COPY ["src/aspire/TeckShop.ServiceDefaults/TeckShop.ServiceDefaults.csproj", "src/aspire/TeckShop.ServiceDefaults/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Core/Teck.Shop.SharedKernel.Core.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Core/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Infrastructure/Teck.Shop.SharedKernel.Infrastructure.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Infrastructure/"]
COPY ["src/services/catalog/Catalog.Application/Catalog.Application.csproj", "src/services/catalog/Catalog.Application/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Events/Teck.Shop.SharedKernel.Events.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Events/"]
COPY ["src/services/catalog/Catalog.Domain/Catalog.Domain.csproj", "src/services/catalog/Catalog.Domain/"]
COPY ["src/services/catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/services/catalog/Catalog.Infrastructure/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Persistence/Teck.Shop.SharedKernel.Persistence.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Persistence/"]
RUN dotnet restore "./src/services/catalog/Catalog.Api/Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/services/catalog/Catalog.Api"
RUN dotnet build "./Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-x64 /p:PublishReadyToRun=true

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64 --self-contained true --no-restore /p:UseAppHost=true /p:PublishReadyToRun=true /p:PublishTrimmed=true

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
