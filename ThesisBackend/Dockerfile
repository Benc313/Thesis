# ============================================================================
# Multi-stage Dockerfile for .NET 9 Backend
# ============================================================================
# 
# This Dockerfile creates an optimized production image for the Rev_n_Roll backend.
# 
# Stages:
# 1. base - Runtime base image
# 2. build - Build environment with SDK
# 3. publish - Published application
# 4. final - Final production image
#
# Build: docker build -t revnroll-backend .
# Run: docker run -p 5000:80 revnroll-backend

# ============================================================================
# STAGE 1: Base Runtime Image
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for healthchecks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# ============================================================================
# STAGE 2: Build Environment
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["ThesisBackend.csproj", "./"]
COPY ["ThesisBackend.Domain/ThesisBackend.Domain.csproj", "ThesisBackend.Domain/"]
COPY ["ThesisBackend.Data/ThesisBackend.Data.csproj", "ThesisBackend.Data/"]
COPY ["ThesisBackend.Services/ThesisBackend.Services.csproj", "ThesisBackend.Services/"]

# Restore dependencies
RUN dotnet restore "ThesisBackend.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src"
RUN dotnet build "ThesisBackend.csproj" -c Release -o /app/build

# ============================================================================
# STAGE 3: Publish Application
# ============================================================================
FROM build AS publish
RUN dotnet publish "ThesisBackend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ============================================================================
# STAGE 4: Final Production Image
# ============================================================================
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Configure environment
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ThesisBackend.dll"]
