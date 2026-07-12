using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.RawMaterial.Commands.Delete;

public record DeleteRawMaterialCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
