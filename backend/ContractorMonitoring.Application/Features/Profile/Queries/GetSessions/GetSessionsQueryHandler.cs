using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetSessions;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, ApiResponse<List<SessionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetSessionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<SessionDto>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) return ApiResponse<List<SessionDto>>.Fail("User not found");
        var sessions = new List<SessionDto>();
        if (user.RefreshToken != null)
            sessions.Add(new SessionDto { Id = Guid.NewGuid(), DeviceInfo = "Current Session", IpAddress = "Current", Location = "Current", LastActivity = user.LastLoginAt ?? DateTime.UtcNow, IsCurrent = true });
        return ApiResponse<List<SessionDto>>.Ok(sessions, "Sessions retrieved");
    }
}