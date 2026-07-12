using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.Application.Features.Auth.Commands.ChangePassword;

// Change password command
public record ChangePasswordCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public ChangePasswordRequest Request { get; init; } = null!;
}