using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UploadPicture;

public class UploadPictureCommandHandler : IRequestHandler<UploadPictureCommand, ApiResponse<ProfilePictureDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public UploadPictureCommandHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<ProfilePictureDto>> Handle(UploadPictureCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
            return ApiResponse<ProfilePictureDto>.Fail("User not found");

        // Validate file
        if (command.File == null || command.File.Length == 0)
            return ApiResponse<ProfilePictureDto>.Fail("No file uploaded");

        // Delete old picture if exists
        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            try { await _fileStorage.DeleteFileAsync(user.ProfilePicture); } catch { }
        }

        // Upload new picture
        var picturePath = await _fileStorage.UploadFileAsync(command.File, "profiles");
        user.ProfilePicture = picturePath;
        user.UpdatedAt = DateTime.UtcNow;
        user.LastProfileUpdate = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProfilePictureDto>.Ok(
            new ProfilePictureDto { PictureUrl = picturePath },
            "Profile picture uploaded successfully");
    }
}