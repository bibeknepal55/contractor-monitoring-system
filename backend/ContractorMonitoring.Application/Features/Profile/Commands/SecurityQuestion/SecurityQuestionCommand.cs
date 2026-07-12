using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Commands.SecurityQuestion;

public record SecurityQuestionCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
    public SecurityQuestionDto Request { get; init; } = null!;
}