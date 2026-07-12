using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Logout;

// Logout command
public record LogoutCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
}