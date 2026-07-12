using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetActivities;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, ApiResponse<List<ActivityDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetActivitiesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<ActivityDto>>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) return ApiResponse<List<ActivityDto>>.Fail("User not found");
        var activities = new List<ActivityDto> { new() { ActivityType = "Login", Description = "Logged in", IpAddress = "Current", CreatedAt = user.LastLoginAt ?? DateTime.UtcNow } };
        if (user.LastPasswordChange.HasValue) activities.Add(new() { ActivityType = "PasswordChange", Description = "Password changed", CreatedAt = user.LastPasswordChange.Value });
        return ApiResponse<List<ActivityDto>>.Ok(activities.OrderByDescending(a => a.CreatedAt).ToList(), "Activities retrieved");
    }
}