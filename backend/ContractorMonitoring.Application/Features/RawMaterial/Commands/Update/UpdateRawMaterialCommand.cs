using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.RawMaterial;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Update;

public record UpdateRawMaterialCommand : IRequest<ApiResponse<RawMaterialDto>>
{
    public Guid Id { get; init; }
    public UpdateRawMaterialDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
