using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.UserManagement;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<UserManagementDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var userExists = await _unitOfWork.Users.ExistsAsync(u => u.Email == command.Request.Email);
        if (userExists)
            return ApiResponse<UserManagementDto>.Fail("User with this email already exists");

        // Validate password strength
        if (string.IsNullOrEmpty(command.Request.Password) || command.Request.Password.Length < 8)
            return ApiResponse<UserManagementDto>.Fail("Password must be at least 8 characters");

        var allUserRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var allRoles = await _unitOfWork.Roles.GetAllAsync();

        // Resolve role names: support both RoleId (single) and Roles (list of names)
        var resolvedRoleNames = new List<string>();
        if (!string.IsNullOrEmpty(command.Request.RoleId) && Guid.TryParse(command.Request.RoleId, out var roleGuid))
        {
            var role = allRoles.FirstOrDefault(r => r.Id == roleGuid && !r.IsDeleted);
            if (role != null) resolvedRoleNames.Add(role.Name);
        }
        else if (command.Request.Roles.Any())
        {
            resolvedRoleNames.AddRange(command.Request.Roles);
        }

        if (!resolvedRoleNames.Any())
            return ApiResponse<UserManagementDto>.Fail("At least one role must be assigned");

        // Get creator's roles for RBAC check
        var creatorRoles = (from ur in allUserRoles
                            join r in allRoles on ur.RoleId equals r.Id
                            where ur.UserId == command.CreatedBy && !ur.IsDeleted
                            select r.Name).ToList();
        var isSuperAdmin = creatorRoles.Contains("SuperAdmin");
        var isAdmin = creatorRoles.Contains("Admin");

        // RBAC: Validate role assignment
        foreach (var roleName in resolvedRoleNames)
        {
            if (isSuperAdmin) continue;
            if (isAdmin && (roleName == "Test" || roleName == "Viewer")) continue;
            // Admin can assign custom (non-system) roles
            var targetRole = allRoles.FirstOrDefault(r => r.Name == roleName && !r.IsDeleted);
            if (isAdmin && targetRole != null && !targetRole.IsSystem) continue;
            if (creatorRoles.Contains("Test") && roleName == "Test") continue;
            return ApiResponse<UserManagementDto>.Fail($"You cannot assign the role: {roleName}");
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Request.Email.ToLower().Trim(),
            PasswordHash = _passwordService.HashPassword(command.Request.Password),
            FirstName = command.Request.FirstName.Trim(),
            LastName = command.Request.LastName.Trim(),
            PhoneNumber = command.Request.PhoneNumber?.Trim(),
            IsActive = command.Request.IsActive,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.CreatedBy.ToString(),
            TenantId = Guid.Empty
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign roles
        foreach (var roleName in resolvedRoleNames)
        {
            var role = allRoles.FirstOrDefault(r => r.Name == roleName && !r.IsDeleted);
            if (role != null)
            {
                await _unitOfWork.UserRoles.AddAsync(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.CreatedBy.ToString(),
                    TenantId = user.TenantId,
                    IsDeleted = false
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<UserManagementDto>.Ok(new UserManagementDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = resolvedRoleNames,
            Permissions = new List<string>(),
            CreatedAt = user.CreatedAt
        }, "User created successfully");
    }
}