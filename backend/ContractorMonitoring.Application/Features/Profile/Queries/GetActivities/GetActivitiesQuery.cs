using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetActivities;

public record GetActivitiesQuery : IRequest<ApiResponse<List<ActivityDto>>>
{
    public Guid UserId { get; init; }
}