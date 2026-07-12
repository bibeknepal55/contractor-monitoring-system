using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.ResponsibleOfficial;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Update;

public record UpdateResponsibleOfficialCommand : IRequest<ApiResponse<ResponsibleOfficialDto>>
{
    public Guid Id { get; init; }
    public UpdateResponsibleOfficialDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
