using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Login;

// Login command
public record LoginCommand : IRequest<ApiResponse<AuthResponse>>
{
    public LoginRequest Request { get; init; } = null!;
}