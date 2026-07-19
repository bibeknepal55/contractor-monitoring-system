using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IPermissionResolver _permissionResolver;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IPermissionResolver permissionResolver)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _permissionResolver = permissionResolver;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var userExists = await _unitOfWork.Users.ExistsAsync(u => u.Email == command.Request.Email);
        if (userExists)
            return ApiResponse<AuthResponse>.Fail("User with this email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Request.Email.ToLower().Trim(),
            PasswordHash = _passwordService.HashPassword(command.Request.Password),
            FirstName = command.Request.FirstName.Trim(),
            LastName = command.Request.LastName.Trim(),
            PhoneNumber = command.Request.PhoneNumber?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            TenantId = Guid.Empty
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var allRoles = await _unitOfWork.Roles.GetAllAsync();
        var defaultRole = allRoles.FirstOrDefault(r => r.Name == "Viewer");
        if (defaultRole != null)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = user.TenantId
            });
            await _unitOfWork.SaveChangesAsync();
        }

        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(accessToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Use centralized PermissionResolver
        var userRoles = await _permissionResolver.GetUserRolesAsync(user.Id);
        var userPermissions = await _permissionResolver.GetUserPermissionsAsync(user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles,
                Permissions = userPermissions
            }
        }, "Registration successful");
    }
}