# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ContractorMonitoring.sln .
COPY ContractorMonitoring.Domain/ContractorMonitoring.Domain.csproj ContractorMonitoring.Domain/
COPY ContractorMonitoring.Application/ContractorMonitoring.Application.csproj ContractorMonitoring.Application/
COPY ContractorMonitoring.Infrastructure/ContractorMonitoring.Infrastructure.csproj ContractorMonitoring.Infrastructure/
COPY ContractorMonitoring.API/ContractorMonitoring.API.csproj ContractorMonitoring.API/

RUN dotnet restore ContractorMonitoring.sln

COPY . .

RUN dotnet publish ContractorMonitoring.API/ContractorMonitoring.API.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

# Ensure uploads directory exists with correct permissions
RUN mkdir -p Uploads/profiles && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ContractorMonitoring.API.dll"]
