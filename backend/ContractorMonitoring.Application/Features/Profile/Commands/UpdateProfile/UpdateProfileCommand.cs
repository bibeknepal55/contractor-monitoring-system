using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<ApiResponse<ProfileDto>>
{
    public Guid UserId { get; init; }
    public UpdateProfileDto Request { get; init; } = null!;
    public string IpAddress { get; init; } = string.Empty;
    public string DeviceInfo { get; init; } = string.Empty;
}