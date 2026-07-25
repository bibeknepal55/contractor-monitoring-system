using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities.Base;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : AuditableEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<PagedResponse<T>> GetPagedAsync(
        PaginationFilter filter,
        Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        // Apply includes
        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        // Apply filter predicate
        if (predicate != null)
            query = query.Where(predicate);

        // FIXED: Apply search only on specific searchable fields per entity
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = ApplySearch(query, filter.Search);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = ApplySorting(query, filter.SortBy, filter.SortOrder);
        }
        else
        {
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResponse<T>
        {
            Data = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };
    }

    // FIXED: Apply search on known searchable fields per entity type
    private IQueryable<T> ApplySearch(IQueryable<T> query, string searchTerm)
    {
        var searchLower = searchTerm.ToLower();
        var entityType = typeof(T).Name;

        // Define searchable fields per entity type
        var searchableFields = GetSearchableFields(entityType);

        if (searchableFields.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? searchExpression = null;

        foreach (var field in searchableFields)
        {
            var property = typeof(T).GetProperty(field);
            if (property != null && property.PropertyType == typeof(string))
            {
                var propertyAccess = Expression.Property(parameter, property);
                var toLower = Expression.Call(propertyAccess, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
                var searchConstant = Expression.Constant(searchLower);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var containsExpression = Expression.Call(toLower, containsMethod!, searchConstant);

                searchExpression = searchExpression == null
                    ? containsExpression
                    : Expression.OrElse(searchExpression, containsExpression);
            }
        }

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    private static string[] GetSearchableFields(string entityName) => entityName switch
    {
        "Project" => new[] { "ProjectName", "ProjectCode", "Description", "Location", "ProjectManager" },
        "ContractorOfficeDetail" => new[] { "CompanyName", "RegistrationNumber", "Email", "ContactPerson" },
        "ContractFinancialDetail" => new[] { "BankName", "PaymentTerms", "Currency" },
        "PriceAdjustment" => new[] { "Reason", "AdjustmentType" },
        "PerformanceBond" => new[] { "BondNumber", "IssuingBank" },
        "AdvancePaymentGuarantee" => new[] { "GuaranteeNumber", "IssuingBank" },
        "PhysicalProgress" => new[] { "ActivityDescription", "ReportedBy" },
        "TimeExtension" => new[] { "ExtensionNumber", "Reason" },
        "DelayReason" => new[] { "Description", "DelayCategory" },
        "RawMaterial" => new[] { "MaterialName", "MaterialCode", "SupplierName" },
        "LabTest" => new[] { "TestName", "TestCode" },
        "PhotoMonitoring" => new[] { "Title", "Description", "Tags" },
        "Subcontractor" => new[] { "CompanyName", "ScopeOfWork" },
        "ResponsibleOfficial" => new[] { "FullName", "Position", "Email" },
        "User" => new[] { "Email", "FirstName", "LastName", "PhoneNumber" },
        _ => Array.Empty<string>()
    };

    private IQueryable<T> ApplySorting(IQueryable<T> query, string sortBy, string? sortOrder)
    {
        var property = typeof(T).GetProperty(sortBy);
        if (property == null) return query.OrderByDescending(e => e.CreatedAt);

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);

        var methodName = sortOrder?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }
}