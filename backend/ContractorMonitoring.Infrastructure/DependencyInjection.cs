using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Infrastructure.Data;
using ContractorMonitoring.Infrastructure.Repositories;
using ContractorMonitoring.Infrastructure.Services;

namespace ContractorMonitoring.Infrastructure;

// Extension methods for registering Infrastructure layer services
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Export Service
        services.AddScoped<IExportService, ExportService>();

        // Register Background Services
        services.AddHostedService<BackgroundJobService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Register Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Approval repositories
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        // Register Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}