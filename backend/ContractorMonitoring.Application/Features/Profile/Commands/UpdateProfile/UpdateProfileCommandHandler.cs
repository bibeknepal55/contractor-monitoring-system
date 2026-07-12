using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<ProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<ProfileDto>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null) return ApiResponse<ProfileDto>.Fail("User not found");

        user.FirstName = command.Request.FirstName;
        user.LastName = command.Request.LastName;
        user.PhoneNumber = command.Request.PhoneNumber;
        user.Bio = command.Request.Bio;
        user.JobTitle = command.Request.JobTitle;
        user.Department = command.Request.Department;
        user.Company = command.Request.Company;
        user.LastProfileUpdate = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProfileDto>.Ok(new ProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfilePicture = user.ProfilePicture,
            Bio = user.Bio,
            JobTitle = user.JobTitle,
            Department = user.Department,
            Company = user.Company,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        }, "Profile updated");
    }
}