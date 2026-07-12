using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Interfaces;

// Unit of Work pattern for transaction management
public interface IUnitOfWork : IDisposable
{
    // Auth repositories
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<Permission> Permissions { get; }
    IGenericRepository<UserRole> UserRoles { get; }
    IGenericRepository<RolePermission> RolePermissions { get; }

    // Business repositories
    IGenericRepository<Project> Projects { get; }
    IGenericRepository<ContractorOfficeDetail> ContractorOfficeDetails { get; }
    IGenericRepository<ContractFinancialDetail> ContractFinancialDetails { get; }
    IGenericRepository<PriceAdjustment> PriceAdjustments { get; }
    IGenericRepository<PerformanceBond> PerformanceBonds { get; }
    IGenericRepository<AdvancePaymentGuarantee> AdvancePaymentGuarantees { get; }
    IGenericRepository<PhysicalProgress> PhysicalProgresses { get; }
    IGenericRepository<TimeExtension> TimeExtensions { get; }
    IGenericRepository<DelayReason> DelayReasons { get; }
    IGenericRepository<RawMaterial> RawMaterials { get; }
    IGenericRepository<LabTest> LabTests { get; }
    IGenericRepository<PhotoMonitoring> PhotoMonitorings { get; }
    IGenericRepository<Subcontractor> Subcontractors { get; }
    IGenericRepository<ResponsibleOfficial> ResponsibleOfficials { get; }
    IGenericRepository<ApprovalWorkflow> ApprovalWorkflows { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}