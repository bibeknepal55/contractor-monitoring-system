using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.ResponsibleOfficial.Commands.Delete;

public record DeleteResponsibleOfficialCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
