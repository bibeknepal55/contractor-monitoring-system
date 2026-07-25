using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Infrastructure.Data;
using ContractorMonitoring.Infrastructure.Repositories;
using ContractorMonitoring.Infrastructure.Services;

namespace ContractorMonitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantService, TenantService>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Unit of Work & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IApprovalRepository, ApprovalRepository>();

        // Core domain services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPermissionResolver, PermissionResolver>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Phase 1 — Security & Compliance
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IGdprService, GdprService>();
        services.AddScoped<ISecurityPolicyService, SecurityPolicyService>();

        // Phase 2 — Multi-Tenancy & Redis Cache
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        services.AddScoped<IPermissionCacheService, PermissionCacheService>();

        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
        }
        else
        {
            // Fallback to in-memory cache when Redis is not configured
            services.AddDistributedMemoryCache();
        }

        // Phase 3 — ABAC + Permission Broadcast
        services.AddScoped<IAbacService, AbacService>();
        services.AddScoped<IPermissionBroadcastService, PermissionBroadcastService>();

        // Phase 4 — BI
        services.AddScoped<IPerformanceScoringService, PerformanceScoringService>();
        services.AddScoped<IPredictiveAlertService, PredictiveAlertService>();

        // Phase 5 — Notifications
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        // Phase 6 — Hangfire (PostgreSQL storage)
        var connStr = configuration.GetConnectionString("DefaultConnection")!;
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connStr)));
        services.AddHangfireServer();

        // Hangfire job classes
        services.AddScoped<ExpiryNotificationJob>();
        services.AddScoped<AuditChainVerificationJob>();
        services.AddScoped<TokenCleanupJob>();

        // Phase 6 — SignalR (built-in, no extra package needed for .NET 8)
        // Registered in Program.cs via builder.Services.AddSignalR()

        return services;
    }
}
