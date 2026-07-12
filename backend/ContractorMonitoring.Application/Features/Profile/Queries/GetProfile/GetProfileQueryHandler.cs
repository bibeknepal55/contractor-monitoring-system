using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Profile;
using ContractorMonitoring.Application.Interfaces;

namespace ContractorMonitoring.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ApiResponse<ProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetProfileQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<ProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null) return ApiResponse<ProfileDto>.Fail("User not found");

        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();

        var userRoleNames = (from ur in userRoles
                             join r in roles on ur.RoleId equals r.Id
                             where ur.UserId == request.UserId && !ur.IsDeleted
                             select r.Name).ToList();
        var userPermissions = (from ur in userRoles
                               join rp in rolePermissions on ur.RoleId equals rp.RoleId
                               join p in permissions on rp.PermissionId equals p.Id
                               where ur.UserId == request.UserId && !ur.IsDeleted && !rp.IsDeleted
                               select p.Name).Distinct().ToList();

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
            Roles = userRoleNames,
            Permissions = userPermissions,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Timezone = user.Timezone ?? "UTC",
            Language = user.Language ?? "en",
            Theme = user.Theme ?? "light",
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications,
            SmsNotifications = user.SmsNotifications,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LastPasswordChange = user.LastPasswordChange,
            HasSecurityQuestion = !string.IsNullOrEmpty(user.SecurityQuestion)
        }, "Profile retrieved");
    }
}