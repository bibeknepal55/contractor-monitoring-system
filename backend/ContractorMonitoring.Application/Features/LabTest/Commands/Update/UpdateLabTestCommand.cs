using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Update;

public record UpdateLabTestCommand : IRequest<ApiResponse<LabTestDto>>
{
    public Guid Id { get; init; }
    public UpdateLabTestDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
}
