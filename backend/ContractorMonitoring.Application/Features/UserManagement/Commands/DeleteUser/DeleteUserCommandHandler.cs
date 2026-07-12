using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
            return ApiResponse<bool>.Fail("User not found");

        // Check if trying to delete SuperAdmin
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();

        var userRoleNames = (from ur in userRoles
                             join r in roles on ur.RoleId equals r.Id
                             where ur.UserId == command.UserId && !ur.IsDeleted
                             select r.Name).ToList();

        if (userRoleNames.Contains("SuperAdmin"))
            return ApiResponse<bool>.Fail("Cannot delete SuperAdmin user");

        // Delete user roles first
        var existingRoles = userRoles.Where(ur => ur.UserId == command.UserId).ToList();
        foreach (var ur in existingRoles)
        {
            await _unitOfWork.UserRoles.DeleteAsync(ur);
        }

        // Hard delete the user
        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "User deleted successfully");
    }
}