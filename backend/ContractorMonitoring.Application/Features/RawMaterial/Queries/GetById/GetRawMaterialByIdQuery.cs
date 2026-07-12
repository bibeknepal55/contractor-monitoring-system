using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;

namespace ContractorMonitoring.Application.Features.RawMaterial.Queries.GetById;

public record GetRawMaterialByIdQuery : IRequest<ApiResponse<RawMaterialDto>>
{
    public Guid Id { get; init; }
}
