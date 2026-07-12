using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.Application.Features.Auth.Commands.RefreshToken;

// Refresh token command
public record RefreshTokenCommand : IRequest<ApiResponse<AuthResponse>>
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}