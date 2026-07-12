using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.SecurityQuestion;

public class SecurityQuestionCommandHandler : IRequestHandler<SecurityQuestionCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public SecurityQuestionCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<bool>> Handle(SecurityQuestionCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");
        if (!_passwordService.VerifyPassword(command.Request.Password, user.PasswordHash))
            return ApiResponse<bool>.Fail("Invalid password");
        user.SecurityQuestion = command.Request.Question;
        user.SecurityAnswerHash = _passwordService.HashPassword(command.Request.Answer.ToLower());
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Security question updated");
    }
}