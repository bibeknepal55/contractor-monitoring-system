using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Create;

public record CreateRawMaterialCommand : IRequest<ApiResponse<RawMaterialDto>>
{
    public CreateRawMaterialDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
