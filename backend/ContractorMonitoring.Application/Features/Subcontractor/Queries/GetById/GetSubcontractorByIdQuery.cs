using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;

namespace ContractorMonitoring.Application.Features.Subcontractor.Queries.GetById;

public record GetSubcontractorByIdQuery : IRequest<ApiResponse<SubcontractorDto>>
{
    public Guid Id { get; init; }
}
