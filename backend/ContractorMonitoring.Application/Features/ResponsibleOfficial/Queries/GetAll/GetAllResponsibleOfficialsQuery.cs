using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetAll;

public record GetAllResponsibleOfficialsQuery : IRequest<PagedResponse<ResponsibleOfficialDto>>
{
    public PaginationFilter Filter { get; init; } = new();
    public Guid TenantId { get; init; }
}
