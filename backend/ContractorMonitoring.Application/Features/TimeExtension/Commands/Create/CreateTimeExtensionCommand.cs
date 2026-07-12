using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Create;

public record CreateTimeExtensionCommand : IRequest<ApiResponse<TimeExtensionDto>>
{
    public CreateTimeExtensionDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
