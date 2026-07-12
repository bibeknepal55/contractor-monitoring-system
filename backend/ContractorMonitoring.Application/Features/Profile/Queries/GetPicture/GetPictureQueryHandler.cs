using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetPicture;

public class GetPictureQueryHandler : IRequestHandler<GetPictureQuery, ApiResponse<ProfilePictureDownloadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public GetPictureQueryHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<ProfilePictureDownloadDto>> Handle(GetPictureQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null)
            return ApiResponse<ProfilePictureDownloadDto>.Fail("User not found");

        if (string.IsNullOrEmpty(user.ProfilePicture))
            return ApiResponse<ProfilePictureDownloadDto>.Fail("No profile picture found");

        try
        {
            var fileBytes = await _fileStorage.DownloadFileAsync(user.ProfilePicture);
            var extension = Path.GetExtension(user.ProfilePicture).ToLower();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return ApiResponse<ProfilePictureDownloadDto>.Ok(
                new ProfilePictureDownloadDto
                {
                    FileBytes = fileBytes,
                    FileName = Path.GetFileName(user.ProfilePicture),
                    ContentType = contentType
                },
                "Profile picture retrieved");
        }
        catch (FileNotFoundException)
        {
            return ApiResponse<ProfilePictureDownloadDto>.Fail("Profile picture file not found on disk");
        }
    }
}