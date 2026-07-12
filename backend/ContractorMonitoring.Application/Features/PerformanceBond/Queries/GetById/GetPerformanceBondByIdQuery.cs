using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.PerformanceBond;

namespace ContractorMonitoring.Application.Features.PerformanceBond.Queries.GetById;

public record GetPerformanceBondByIdQuery : IRequest<ApiResponse<PerformanceBondDto>>
{
    public Guid Id { get; init; }
}
