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
COPY ["src/Directory.Build.props", "src/"]
COPY ["Directory.Build.props", "."]
COPY ["src/gateways/Yarp.Gateway.Public/Yarp.Gateway.Public.csproj", "src/gateways/Yarp.Gateway.Public/"]

# Use Docker Buildx and TARGETPLATFORM to select the correct RID for multi-platform builds
ARG TARGETPLATFORM
# Default to x64 if not set
ARG RUNTIME_IDENTIFIER=linux-musl-x64

# Map TARGETPLATFORM to .NET RID
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet restore "src/gateways/Yarp.Gateway.Public/Yarp.Gateway.Public.csproj" -r $RID

COPY . .
WORKDIR "/src/src/gateways/Yarp.Gateway.Public"
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet build "Yarp.Gateway.Public.csproj" -c $BUILD_CONFIGURATION -o /app/build -r $RID /p:PublishReadyToRun=true

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETPLATFORM
ARG RUNTIME_IDENTIFIER=linux-musl-x64
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ]; then export RID=linux-musl-arm64; else export RID=linux-musl-x64; fi && \
    dotnet publish "Yarp.Gateway.Public.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r $RID --self-contained true --no-restore /p:UseAppHost=true /p:PublishReadyToRun=true /p:PublishTrimmed=true

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yarp.Gateway.Public.dll"]
