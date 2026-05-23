FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy entire source for restore and build
COPY . .

# Restore and publish in single layer (optimized)
RUN dotnet publish ./src/API/API.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Add non-root user for security
RUN useradd -m -u 1001 appuser

# Copy published application
COPY --from=build /app/out .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD dotnet --version || exit 1

USER appuser
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]