using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Register;

// Register command
public record RegisterCommand : IRequest<ApiResponse<AuthResponse>>
{
    public RegisterRequest Request { get; init; } = null!;
}