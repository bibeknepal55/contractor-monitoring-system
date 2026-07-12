using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;

namespace ContractorMonitoring.Application.Features.LabTest.Commands.Create;

public record CreateLabTestCommand : IRequest<ApiResponse<LabTestDto>>
{
    public CreateLabTestDto Request { get; init; } = null!;
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
}
