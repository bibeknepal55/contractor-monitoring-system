using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PhotoMonitoring;

namespace ContractorMonitoring.Application.Features.PhotoMonitoring.Queries.GetById;

public record GetPhotoMonitoringByIdQuery : IRequest<ApiResponse<PhotoMonitoringDto>>
{
    public Guid Id { get; init; }
}
