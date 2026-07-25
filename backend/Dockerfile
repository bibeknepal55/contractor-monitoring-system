FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only API project files
COPY ContractorMonitoring.API/ContractorMonitoring.API.csproj ContractorMonitoring.API/
COPY ContractorMonitoring.Application/ContractorMonitoring.Application.csproj ContractorMonitoring.Application/
COPY ContractorMonitoring.Domain/ContractorMonitoring.Domain.csproj ContractorMonitoring.Domain/
COPY ContractorMonitoring.Infrastructure/ContractorMonitoring.Infrastructure.csproj ContractorMonitoring.Infrastructure/

# Restore only the API project
RUN dotnet restore ContractorMonitoring.API/ContractorMonitoring.API.csproj

# Copy everything
COPY . .

# Publish only the API project
WORKDIR /app/ContractorMonitoring.API
RUN dotnet publish -c Release -o /out --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "ContractorMonitoring.API.dll"]