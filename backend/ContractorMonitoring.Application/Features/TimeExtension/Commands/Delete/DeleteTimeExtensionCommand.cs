using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.TimeExtension.Commands.Delete;

public record DeleteTimeExtensionCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
