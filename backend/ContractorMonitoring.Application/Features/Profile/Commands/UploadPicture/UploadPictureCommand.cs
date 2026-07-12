using MediatR;
using Microsoft.AspNetCore.Http;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UploadPicture;

public record UploadPictureCommand : IRequest<ApiResponse<ProfilePictureDto>>
{
    public Guid UserId { get; init; }
    public IFormFile File { get; init; } = null!;
}