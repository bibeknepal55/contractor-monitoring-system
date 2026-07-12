using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Infrastructure.Data;

// Application database context with audit trail and soft delete support
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Authentication & Authorization tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    // Project Management tables
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatic audit trail population
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;

                    // Prevent modification of CreatedAt and CreatedBy
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    break;
            }
        }

        // Soft delete handling - convert Delete to Update with IsDeleted flag
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
                     .Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}