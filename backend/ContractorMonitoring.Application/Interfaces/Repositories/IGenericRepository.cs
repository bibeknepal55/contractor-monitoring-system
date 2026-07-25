using System.Linq.Expressions;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Interfaces.Repositories;

// Generic repository interface for basic CRUD operations
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedResponse<T>> GetPagedAsync(
        PaginationFilter filter,
        Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SoftDeleteAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}