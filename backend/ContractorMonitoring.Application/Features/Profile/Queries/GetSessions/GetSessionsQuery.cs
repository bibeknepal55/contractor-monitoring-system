using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetSessions;

public record GetSessionsQuery : IRequest<ApiResponse<List<SessionDto>>>
{
    public Guid UserId { get; init; }
}