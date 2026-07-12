using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.DeletePicture;

public class DeletePictureCommandHandler : IRequestHandler<DeletePictureCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public DeletePictureCommandHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<bool>> Handle(DeletePictureCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
            return ApiResponse<bool>.Fail("User not found");

        if (string.IsNullOrEmpty(user.ProfilePicture))
            return ApiResponse<bool>.Fail("No profile picture to delete");

        // Delete file from storage
        try { await _fileStorage.DeleteFileAsync(user.ProfilePicture); } catch { }

        // Clear reference
        user.ProfilePicture = null;
        user.UpdatedAt = DateTime.UtcNow;
        user.LastProfileUpdate = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Profile picture deleted successfully");
    }
}