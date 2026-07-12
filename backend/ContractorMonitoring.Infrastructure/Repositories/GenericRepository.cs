using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities.Base;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Repositories;

// Generic repository implementation with all CRUD operations
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

        // Apply includes for eager loading
        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        // Apply filtering
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            if (properties.Any())
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                Expression? searchExpression = null;

                foreach (var property in properties)
                {
                    var propertyAccess = Expression.Property(parameter, property);
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                    if (containsMethod != null)
                    {
                        var searchConstant = Expression.Constant(searchTerm);
                        var containsExpression = Expression.Call(propertyAccess, containsMethod, searchConstant);

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
            }
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var property = typeof(T).GetProperty(filter.SortBy);
            if (property != null)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var lambda = Expression.Lambda(propertyAccess, parameter);

                var methodName = filter.SortOrder?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.PropertyType);

                query = (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
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