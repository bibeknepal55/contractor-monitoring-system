using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.RoleManagement.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
        if (role == null)
            return ApiResponse<bool>.Fail("Role not found");

        // System roles cannot be deleted
        if (role.IsSystem || role.Name == "SuperAdmin" || role.Name == "Admin" || role.Name == "Viewer")
            return ApiResponse<bool>.Fail("System roles cannot be deleted");

        // Check if users are assigned to this role
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var assignedUsers = userRoles.Count(ur => ur.RoleId == request.RoleId && !ur.IsDeleted);
        if (assignedUsers > 0)
            return ApiResponse<bool>.Fail($"Cannot delete role. {assignedUsers} users are assigned to this role. Reassign them first.");

        // Remove role permissions
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var toRemove = rolePermissions.Where(rp => rp.RoleId == request.RoleId).ToList();
        foreach (var rp in toRemove)
            await _unitOfWork.RolePermissions.DeleteAsync(rp);

        // Delete role
        await _unitOfWork.Roles.SoftDeleteAsync(request.RoleId);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Role deleted successfully");
    }
}