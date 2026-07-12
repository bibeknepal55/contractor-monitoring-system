using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetPicture;

public record GetPictureQuery : IRequest<ApiResponse<ProfilePictureDownloadDto>>
{
    public Guid UserId { get; init; }
}