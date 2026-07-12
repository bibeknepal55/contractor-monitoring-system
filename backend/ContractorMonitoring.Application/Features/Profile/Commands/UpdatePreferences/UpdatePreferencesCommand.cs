using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UpdatePreferences;

public record UpdatePreferencesCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public UpdatePreferencesDto Request { get; init; } = null!;
}