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

        var dtos = new List<UserManagementDto>();
        foreach (var user in paged.Data)
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
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
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