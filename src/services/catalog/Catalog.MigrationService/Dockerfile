# Multi-stage Dockerfile for Catalog.MigrationService
# Build and publish for multi-platform, optimized for Docker cache

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files and props for restore layer (maximizes cache)
COPY ["Directory.Packages.props", "."]
COPY ["src/services/catalog/Directory.Build.props", "src/services/catalog/"]
COPY ["src/Directory.Build.props", "src/"]
COPY ["Directory.Build.props", "."]
COPY ["src/services/catalog/Catalog.MigrationService/Catalog.MigrationService.csproj", "src/services/catalog/Catalog.MigrationService/"]
COPY ["src/aspire/TeckShop.ServiceDefaults/TeckShop.ServiceDefaults.csproj", "src/aspire/TeckShop.ServiceDefaults/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Core/Teck.Shop.SharedKernel.Core.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Core/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Infrastructure/Teck.Shop.SharedKernel.Infrastructure.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Infrastructure/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Persistence/Teck.Shop.SharedKernel.Persistence.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Persistence/"]
COPY ["src/services/catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/services/catalog/Catalog.Infrastructure/"]
COPY ["src/services/catalog/Catalog.Application/Catalog.Application.csproj", "src/services/catalog/Catalog.Application/"]
COPY ["src/buildingblocks/Teck.Shop.SharedKernel.Events/Teck.Shop.SharedKernel.Events.csproj", "src/buildingblocks/Teck.Shop.SharedKernel.Events/"]
COPY ["src/services/catalog/Catalog.Domain/Catalog.Domain.csproj", "src/services/catalog/Catalog.Domain/"]

# Use Docker Buildx and TARGETPLATFORM to select the correct RID for multi-platform builds
ARG TARGETPLATFORM
# Default to x64 if not set
ARG RUNTIME_IDENTIFIER=linux-musl-x64

# Map TARGETPLATFORM to .NET RID
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet restore "src/services/catalog/Catalog.MigrationService/Catalog.MigrationService.csproj" -r $RID

# Copy everything else and build
COPY . .
WORKDIR "/src/src/services/catalog/Catalog.MigrationService"
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet build "Catalog.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/build -r $RID /p:PublishReadyToRun=true

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETPLATFORM
ARG RUNTIME_IDENTIFIER=linux-musl-x64
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet publish "Catalog.MigrationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r $RID --self-contained true --no-restore /p:UseAppHost=true /p:PublishReadyToRun=true /p:PublishTrimmed=true

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-composite AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Set user only in final stage (if needed)
# USER $APP_UID
# ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Catalog.MigrationService.dll"]
