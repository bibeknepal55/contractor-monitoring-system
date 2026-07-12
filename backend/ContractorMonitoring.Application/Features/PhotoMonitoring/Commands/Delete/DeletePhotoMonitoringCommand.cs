using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Delete;

public record DeletePhotoMonitoringCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
