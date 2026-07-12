using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;

namespace ContractorMonitoring.Application.Features.TimeExtension.Queries.GetById;

public record GetTimeExtensionByIdQuery : IRequest<ApiResponse<TimeExtensionDto>>
{
    public Guid Id { get; init; }
}
