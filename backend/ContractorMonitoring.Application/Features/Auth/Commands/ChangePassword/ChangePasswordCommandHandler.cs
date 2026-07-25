using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Auth.Commands.ChangePassword;

// Handler for changing password
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);

        if (user == null)
        {
            return ApiResponse<bool>.Fail("User not found");
        }

        // Verify current password
        if (!_passwordService.VerifyPassword(command.Request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<bool>.Fail("Current password is incorrect");
        }

        // Update password and invalidate all existing sessions
        user.PasswordHash = _passwordService.HashPassword(command.Request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.LastPasswordChange = DateTime.UtcNow;
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.MustChangePassword = false;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password changed successfully");
    }
}