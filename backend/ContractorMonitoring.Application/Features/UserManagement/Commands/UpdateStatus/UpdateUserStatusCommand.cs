using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.UpdateStatus;

public record UpdateUserStatusCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
}