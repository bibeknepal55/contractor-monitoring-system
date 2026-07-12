using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Commands.Create;

public record CreatePhotoMonitoringCommand : IRequest<ApiResponse<PhotoMonitoringDto>>
{
    public CreatePhotoMonitoringDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
