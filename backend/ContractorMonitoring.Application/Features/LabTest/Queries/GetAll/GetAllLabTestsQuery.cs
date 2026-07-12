using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;

namespace ContractorMonitoring.Application.Features.LabTest.Queries.GetAll;

public record GetAllLabTestsQuery : IRequest<PagedResponse<LabTestDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
