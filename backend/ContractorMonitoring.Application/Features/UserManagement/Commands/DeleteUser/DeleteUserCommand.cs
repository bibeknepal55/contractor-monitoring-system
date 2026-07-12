using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.UserManagement.Commands.DeleteUser;

public record DeleteUserCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public Guid DeletedBy { get; init; }
}