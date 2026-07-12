using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UpdatePreferences;

public class UpdatePreferencesCommandHandler : IRequestHandler<UpdatePreferencesCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdatePreferencesCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<bool>> Handle(UpdatePreferencesCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");
        user.Timezone = command.Request.Timezone;
        user.Language = command.Request.Language;
        user.Theme = command.Request.Theme;
        user.EmailNotifications = command.Request.EmailNotifications;
        user.PushNotifications = command.Request.PushNotifications;
        user.SmsNotifications = command.Request.SmsNotifications;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Preferences updated");
    }
}