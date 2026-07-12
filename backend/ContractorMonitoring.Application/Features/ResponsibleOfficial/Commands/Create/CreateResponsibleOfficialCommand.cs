using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Create;

public record CreateResponsibleOfficialCommand : IRequest<ApiResponse<ResponsibleOfficialDto>>
{
    public CreateResponsibleOfficialDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
