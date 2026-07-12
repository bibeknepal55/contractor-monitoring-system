using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.Profile.Commands.RevokeSession;

public record RevokeSessionCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public Guid SessionId { get; init; }
}