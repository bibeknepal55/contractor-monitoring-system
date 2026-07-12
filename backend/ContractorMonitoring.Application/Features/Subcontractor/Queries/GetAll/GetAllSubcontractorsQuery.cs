using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Subcontractor;

namespace ContractorMonitoring.Application.Features.Subcontractor.Queries.GetAll;

public record GetAllSubcontractorsQuery : IRequest<PagedResponse<SubcontractorDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
