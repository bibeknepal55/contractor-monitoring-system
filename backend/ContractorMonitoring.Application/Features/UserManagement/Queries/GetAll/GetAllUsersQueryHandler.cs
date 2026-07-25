using AutoMapper;
using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.UserManagement.Queries.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<UserManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<UserManagementDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.Users.GetPagedAsync(request.Filter);

        // Apply isActive filter post-paging (in-memory since GetPagedAsync doesn't support it)
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        IEnumerable<ContractorMonitoring.Domain.Entities.User> filtered = allUsers;

        if (!string.IsNullOrEmpty(request.IsActiveFilter) && bool.TryParse(request.IsActiveFilter, out var isActive))
            filtered = filtered.Where(u => u.IsActive == isActive);

        // Apply role filter
        if (!string.IsNullOrEmpty(request.RoleFilter))
        {
            var allUserRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var allRoles = await _unitOfWork.Roles.GetAllAsync();
            var userIdsWithRole = (from ur in allUserRoles
                                   join r in allRoles on ur.RoleId equals r.Id
                                   where r.Name == request.RoleFilter && !ur.IsDeleted
                                   select ur.UserId).ToHashSet();
            filtered = filtered.Where(u => userIdsWithRole.Contains(u.Id));
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var s = request.Filter.Search.ToLower();
            filtered = filtered.Where(u =>
                u.Email.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(s)));
        }

        var totalCount = filtered.Count();
        var users = filtered
            .OrderByDescending(u => u.CreatedAt)
            .Skip(request.Filter.Skip)
            .Take(request.Filter.PageSize)
            .ToList();

        var dtos = new List<UserManagementDto>();
        foreach (var user in users)
        {
            var userRoles = await GetUserRoles(user.Id);
            var userPermissions = await GetUserPermissions(user.Id);

            dtos.Add(new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                Roles = userRoles,
                Permissions = userPermissions,
                CreatedAt = user.CreatedAt
            });
        }

        return new PagedResponse<UserManagementDto>
        {
            Data = dtos,
            Page = request.Filter.Page,
            PageSize = request.Filter.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.Filter.PageSize),
            Message = "Users retrieved successfully"
        };
    }

    private async Task<List<string>> GetUserRoles(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return (from ur in userRoles
                join r in roles on ur.RoleId equals r.Id
                where ur.UserId == userId && !ur.IsDeleted
                select r.Name).ToList();
    }

    private async Task<List<string>> GetUserPermissions(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        return (from ur in userRoles
                join rp in rolePermissions on ur.RoleId equals rp.RoleId
                join p in permissions on rp.PermissionId equals p.Id
                where ur.UserId == userId && !ur.IsDeleted && !rp.IsDeleted
                select p.Name).Distinct().ToList();
    }
}