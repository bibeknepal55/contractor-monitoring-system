using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Queries.GetById;

public record GetResponsibleOfficialByIdQuery : IRequest<ApiResponse<ResponsibleOfficialDto>>
{
    public Guid Id { get; init; }
}
