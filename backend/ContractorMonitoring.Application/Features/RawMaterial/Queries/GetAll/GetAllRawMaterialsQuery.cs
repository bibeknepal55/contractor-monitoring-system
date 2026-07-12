using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;

namespace ContractorMonitoring.Application.Features.RawMaterial.Queries.GetAll;

public record GetAllRawMaterialsQuery : IRequest<PagedResponse<RawMaterialDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
