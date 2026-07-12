using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetProfile;

public record GetProfileQuery : IRequest<ApiResponse<ProfileDto>>
{
    public Guid UserId { get; init; }
}