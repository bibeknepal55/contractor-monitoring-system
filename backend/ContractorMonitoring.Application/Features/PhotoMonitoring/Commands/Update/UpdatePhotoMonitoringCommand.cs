using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Update;

public record UpdatePhotoMonitoringCommand : IRequest<ApiResponse<PhotoMonitoringDto>>
{
    public Guid Id { get; init; }
    public UpdatePhotoMonitoringDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
