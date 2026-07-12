using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Commands.TwoFactor;

public record TwoFactorCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public TwoFactorSetupDto Request { get; init; } = null!;
}