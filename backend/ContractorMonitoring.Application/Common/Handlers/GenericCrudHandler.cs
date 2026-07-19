using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Application.Common.Handlers;

// Generic Create Handler
public abstract class GenericCreateHandler<TEntity, TCommand, TDto>
    where TEntity : AuditableEntity
    where TCommand : IRequest<ApiResponse<TDto>>
{
    protected readonly IUnitOfWork UnitOfWork;

    protected GenericCreateHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected abstract TEntity MapToEntity(TCommand command);
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract Task AddEntityAsync(TEntity entity);

    protected virtual async Task<ApiResponse<TDto>> Handle(TCommand command)
    {
        var entity = MapToEntity(command);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        await AddEntityAsync(entity);
        await UnitOfWork.SaveChangesAsync();

        var dto = MapToDto(entity);
        return ApiResponse<TDto>.Ok(dto, $"{typeof(TEntity).Name} created successfully");
    }
}

// Generic Update Handler
public abstract class GenericUpdateHandler<TEntity, TCommand, TDto>
    where TEntity : AuditableEntity
    where TCommand : IRequest<ApiResponse<TDto>>
{
    protected readonly IUnitOfWork UnitOfWork;

    protected GenericUpdateHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected abstract Task<TEntity?> FindEntityAsync(TCommand command);
    protected abstract void UpdateEntity(TEntity entity, TCommand command);
    protected abstract TDto MapToDto(TEntity entity);

    protected virtual async Task<ApiResponse<TDto>> Handle(TCommand command)
    {
        var entity = await FindEntityAsync(command);
        if (entity == null)
            return ApiResponse<TDto>.Fail($"{typeof(TEntity).Name} not found");

        UpdateEntity(entity, command);
        entity.UpdatedAt = DateTime.UtcNow;
        await UnitOfWork.SaveChangesAsync();

        var dto = MapToDto(entity);
        return ApiResponse<TDto>.Ok(dto, $"{typeof(TEntity).Name} updated successfully");
    }
}

// Generic Delete Handler
public abstract class GenericDeleteHandler<TEntity, TCommand>
    where TEntity : AuditableEntity
    where TCommand : IRequest<ApiResponse<bool>>
{
    protected readonly IUnitOfWork UnitOfWork;

    protected GenericDeleteHandler(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    protected abstract Task<TEntity?> FindEntityAsync(TCommand command);

    protected virtual async Task<ApiResponse<bool>> Handle(TCommand command)
    {
        var entity = await FindEntityAsync(command);
        if (entity == null)
            return ApiResponse<bool>.Fail($"{typeof(TEntity).Name} not found");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await UnitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"{typeof(TEntity).Name} deleted successfully");
    }
}