using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.TimeExtension;

namespace ContractorMonitoring.Application.Features.TimeExtension.Queries.GetAll;

public record GetAllTimeExtensionsQuery : IRequest<PagedResponse<TimeExtensionDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
