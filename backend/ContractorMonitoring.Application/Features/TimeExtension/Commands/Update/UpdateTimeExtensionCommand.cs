using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Update;

public record UpdateTimeExtensionCommand : IRequest<ApiResponse<TimeExtensionDto>>
{
    public Guid Id { get; init; }
    public UpdateTimeExtensionDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
