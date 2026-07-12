using MediatR;
using ContractorMonitoring.Application.Common.Models;

namespace ContractorMonitoring.Application.Features.Profile.Commands.DeletePicture;

public record DeletePictureCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; init; }
}