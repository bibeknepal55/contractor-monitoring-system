using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Domain.Entities.Base;
using ContractorMonitoring.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ContractorMonitoring.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService? _tenantService;

    // Runtime constructor: ITenantService resolves tenant at query time (not model-creation time)
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService? tenantService = null) : base(options)
    {
        _tenantService = tenantService;
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

    // Phase 1 — Security & Compliance
    public DbSet<AuditTrail> AuditTrails { get; set; } = null!;
    public DbSet<GdprRequest> GdprRequests { get; set; } = null!;

    // Phase 2 — Multi-Tenancy
    public DbSet<Tenant> Tenants { get; set; } = null!;

    // Phase 3 — Advanced PBAC
    public DbSet<ResourcePolicy> ResourcePolicies { get; set; } = null!;
    public DbSet<TimeBoundUserRole> TimeBoundUserRoles { get; set; } = null!;
    public DbSet<RoleInheritance> RoleInheritances { get; set; } = null!;

    // Phase 4 — Business Intelligence
    public DbSet<ContractorPerformanceScore> ContractorPerformanceScores { get; set; } = null!;
    public DbSet<PredictiveAlert> PredictiveAlerts { get; set; } = null!;

    // Phase 5 — Collaboration
    public DbSet<ProjectDocument> ProjectDocuments { get; set; } = null!;
    public DbSet<RecordComment> RecordComments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var hasTenantId = entityType.FindProperty("TenantId") != null;

            // Combine soft-delete + tenant filter in ONE HasQueryFilter call (EF Core only supports one per entity)
            // The lambda closes over _tenantService which is resolved at query time, not model-creation time
            var tenantService = _tenantService;
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

            // !e.IsDeleted
            var isDeletedProp = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
            var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProp);

            System.Linq.Expressions.Expression combined = notDeleted;

            if (hasTenantId)
            {
                // e.TenantId == tenantService.CurrentTenantId.Value  (only when tenant is set)
                var tenantIdProp = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var getTenantId = System.Linq.Expressions.Expression.Call(
                    System.Linq.Expressions.Expression.Constant(tenantService),
                    typeof(ITenantService).GetProperty(nameof(ITenantService.CurrentTenantId))!.GetGetMethod()!);
                var hasValue = System.Linq.Expressions.Expression.Property(getTenantId, "HasValue");
                var tenantValue = System.Linq.Expressions.Expression.Property(getTenantId, "Value");
                var tenantEqual = System.Linq.Expressions.Expression.Equal(tenantIdProp, tenantValue);
                // Apply tenant filter only when CurrentTenantId has a value
                var tenantFilter = System.Linq.Expressions.Expression.AndAlso(
                    System.Linq.Expressions.Expression.IsTrue(hasValue), tenantEqual);
                var noTenantFilter = System.Linq.Expressions.Expression.Not(hasValue);
                var tenantOrSkip = System.Linq.Expressions.Expression.OrElse(noTenantFilter, tenantFilter);
                combined = System.Linq.Expressions.Expression.AndAlso(notDeleted, tenantOrSkip);
            }

            var lambda = System.Linq.Expressions.Expression.Lambda(combined, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
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