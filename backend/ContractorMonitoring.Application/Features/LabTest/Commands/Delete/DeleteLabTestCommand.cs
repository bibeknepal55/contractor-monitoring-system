using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Delete;

public record DeleteLabTestCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; init; }
}
