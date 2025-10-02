# Multi-stage build for Motify Trial Balance Classifier

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/Motify.Core/Motify.Core.csproj src/Motify.Core/
COPY src/Motify.Server/Motify.Server.csproj src/Motify.Server/
RUN dotnet restore src/Motify.Server/Motify.Server.csproj

# Copy source code
COPY src/ src/
COPY config/ config/

# Build and publish
WORKDIR /src/src/Motify.Server
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Copy config
COPY config/config.yaml ./config/

# Create directories for persistent storage
RUN mkdir -p /app/data /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV PORT=5000
ENV DOTNET_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/ || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "motify-server.dll"]
