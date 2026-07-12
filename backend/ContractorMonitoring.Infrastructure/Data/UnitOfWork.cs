using Microsoft.EntityFrameworkCore.Storage;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Repositories;

namespace ContractorMonitoring.Infrastructure.Data;

// Unit of Work implementation with transaction support
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Auth repositories
    private IGenericRepository<User>? _users;
    private IGenericRepository<Role>? _roles;
    private IGenericRepository<Permission>? _permissions;
    private IGenericRepository<UserRole>? _userRoles;
    private IGenericRepository<RolePermission>? _rolePermissions;

    // Business repositories
    private IGenericRepository<Project>? _projects;
    private IGenericRepository<ContractorOfficeDetail>? _contractorOfficeDetails;
    private IGenericRepository<ContractFinancialDetail>? _contractFinancialDetails;
    private IGenericRepository<PriceAdjustment>? _priceAdjustments;
    private IGenericRepository<PerformanceBond>? _performanceBonds;
    private IGenericRepository<AdvancePaymentGuarantee>? _advancePaymentGuarantees;
    private IGenericRepository<PhysicalProgress>? _physicalProgresses;
    private IGenericRepository<TimeExtension>? _timeExtensions;
    private IGenericRepository<DelayReason>? _delayReasons;
    private IGenericRepository<RawMaterial>? _rawMaterials;
    private IGenericRepository<LabTest>? _labTests;
    private IGenericRepository<PhotoMonitoring>? _photoMonitorings;
    private IGenericRepository<Subcontractor>? _subcontractors;
    private IGenericRepository<ResponsibleOfficial>? _responsibleOfficials;
    private IGenericRepository<ApprovalWorkflow>? _approvalWorkflows;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Auth repositories
    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    public IGenericRepository<Role> Roles =>
        _roles ??= new GenericRepository<Role>(_context);

    public IGenericRepository<Permission> Permissions =>
        _permissions ??= new GenericRepository<Permission>(_context);

    public IGenericRepository<UserRole> UserRoles =>
        _userRoles ??= new GenericRepository<UserRole>(_context);

    public IGenericRepository<RolePermission> RolePermissions =>
        _rolePermissions ??= new GenericRepository<RolePermission>(_context);

    // Business repositories
    public IGenericRepository<Project> Projects =>
        _projects ??= new GenericRepository<Project>(_context);

    public IGenericRepository<ContractorOfficeDetail> ContractorOfficeDetails =>
        _contractorOfficeDetails ??= new GenericRepository<ContractorOfficeDetail>(_context);

    public IGenericRepository<ContractFinancialDetail> ContractFinancialDetails =>
        _contractFinancialDetails ??= new GenericRepository<ContractFinancialDetail>(_context);

    public IGenericRepository<PriceAdjustment> PriceAdjustments =>
        _priceAdjustments ??= new GenericRepository<PriceAdjustment>(_context);

    public IGenericRepository<PerformanceBond> PerformanceBonds =>
        _performanceBonds ??= new GenericRepository<PerformanceBond>(_context);

    public IGenericRepository<AdvancePaymentGuarantee> AdvancePaymentGuarantees =>
        _advancePaymentGuarantees ??= new GenericRepository<AdvancePaymentGuarantee>(_context);

    public IGenericRepository<PhysicalProgress> PhysicalProgresses =>
        _physicalProgresses ??= new GenericRepository<PhysicalProgress>(_context);

    public IGenericRepository<TimeExtension> TimeExtensions =>
        _timeExtensions ??= new GenericRepository<TimeExtension>(_context);

    public IGenericRepository<DelayReason> DelayReasons =>
        _delayReasons ??= new GenericRepository<DelayReason>(_context);

    public IGenericRepository<RawMaterial> RawMaterials =>
        _rawMaterials ??= new GenericRepository<RawMaterial>(_context);

    public IGenericRepository<LabTest> LabTests =>
        _labTests ??= new GenericRepository<LabTest>(_context);

    public IGenericRepository<PhotoMonitoring> PhotoMonitorings =>
        _photoMonitorings ??= new GenericRepository<PhotoMonitoring>(_context);

    public IGenericRepository<Subcontractor> Subcontractors =>
        _subcontractors ??= new GenericRepository<Subcontractor>(_context);

    public IGenericRepository<ResponsibleOfficial> ResponsibleOfficials =>
        _responsibleOfficials ??= new GenericRepository<ResponsibleOfficial>(_context);

    public IGenericRepository<ApprovalWorkflow> ApprovalWorkflows =>
        _approvalWorkflows ??= new GenericRepository<ApprovalWorkflow>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}