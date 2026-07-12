using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.LabTest;

namespace ContractorMonitoring.Application.Features.LabTest.Queries.GetById;

public record GetLabTestByIdQuery : IRequest<ApiResponse<LabTestDto>>
{
    public Guid Id { get; init; }
}
