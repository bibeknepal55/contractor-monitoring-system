using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Domain.Entities.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly Guid? _currentTenantId;

    // Runtime constructor: Injects IHttpContextAccessor to resolve the current tenant
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        if (httpContextAccessor?.HttpContext?.Items["TenantId"] is Guid tenantId)
        {
            _currentTenantId = tenantId;
        }
    }

    // Design-time constructor: Used by EF Core CLI for migrations (no HTTP context)
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Authentication & Authorization tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    // Business / Project Management tables
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ContractorOfficeDetail> ContractorOfficeDetails { get; set; } = null!;
    public DbSet<ContractFinancialDetail> ContractFinancialDetails { get; set; } = null!;
    public DbSet<PriceAdjustment> PriceAdjustments { get; set; } = null!;
    public DbSet<PerformanceBond> PerformanceBonds { get; set; } = null!;
    public DbSet<AdvancePaymentGuarantee> AdvancePaymentGuarantees { get; set; } = null!;
    public DbSet<PhysicalProgress> PhysicalProgresses { get; set; } = null!;
    public DbSet<TimeExtension> TimeExtensions { get; set; } = null!;
    public DbSet<DelayReason> DelayReasons { get; set; } = null!;
    public DbSet<RawMaterial> RawMaterials { get; set; } = null!;
    public DbSet<LabTest> LabTests { get; set; } = null!;
    public DbSet<PhotoMonitoring> PhotoMonitorings { get; set; } = null!;
    public DbSet<Subcontractor> Subcontractors { get; set; } = null!;
    public DbSet<ResponsibleOfficial> ResponsibleOfficials { get; set; } = null!;
    public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; } = null!;
    public DbSet<UserActivityLog> UserActivityLogs { get; set; } = null!;

    // Phase 3 Enterprise tables
    public DbSet<RolePermissionHistory> RolePermissionHistories { get; set; } = null!;
    public DbSet<RevokedToken> RevokedTokens { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        var currentTenantId = _currentTenantId;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // 1. GLOBAL SOFT DELETE FILTER - automatically filters IsDeleted=true rows
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
                var notDeleted = System.Linq.Expressions.Expression.Not(property);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }

            // 2. GLOBAL MULTI-TENANCY FILTER - automatically scopes queries by tenant
            var tenantIdProperty = entityType.FindProperty("TenantId");
            if (tenantIdProperty != null && tenantIdProperty.ClrType == typeof(Guid) && currentTenantId.HasValue)
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var constant = System.Linq.Expressions.Expression.Constant(currentTenantId.Value, typeof(Guid));
                var equal = System.Linq.Expressions.Expression.Equal(property, constant);
                var lambda = System.Linq.Expressions.Expression.Lambda(equal, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    break;
            }
        }

        // Soft delete: convert Delete to Update with IsDeleted flag
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>().Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    // Helper to bypass soft-delete filter for admin purge/recovery operations
    public IQueryable<T> WithoutSoftDelete<T>() where T : AuditableEntity
    {
        return Set<T>().IgnoreQueryFilters();
    }
}