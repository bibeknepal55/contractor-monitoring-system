using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.TwoFactor;

public class TwoFactorCommandHandler : IRequestHandler<TwoFactorCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public TwoFactorCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<bool>> Handle(TwoFactorCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");
        if (!_passwordService.VerifyPassword(command.Request.Password, user.PasswordHash))
            return ApiResponse<bool>.Fail("Invalid password");
        user.TwoFactorEnabled = command.Request.Enable;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, $"2FA {(command.Request.Enable ? "enabled" : "disabled")}");
    }
}