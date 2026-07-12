using MediatR;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Features.Auth.Commands.Register;

// Handler for user registration
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var userExists = await _unitOfWork.Users.ExistsAsync(u => u.Email == command.Request.Email);
        if (userExists)
        {
            return ApiResponse<AuthResponse>.Fail("User with this email already exists");
        }

        // Create new user
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
            TenantId = Guid.Empty  // Shared main tenant for all users
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign default "Viewer" role
        var viewerRole = await _unitOfWork.Roles.GetAllAsync();
        var defaultRole = viewerRole.FirstOrDefault(r => r.Name == "Viewer");

        if (defaultRole != null)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = user.TenantId
            };

            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        // Generate tokens
        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateRefreshToken();
        var expiresAt = await _jwtService.GetTokenExpiryTime(accessToken);

        // Update user with refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Prepare response
        var authResponse = new AuthResponse
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
                Roles = new List<string> { "Viewer" },
                Permissions = await GetUserPermissions(user.Id)
            }
        };

        return ApiResponse<AuthResponse>.Ok(authResponse, "Registration successful");
    }

    private async Task<List<string>> GetUserPermissions(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();

        return (from ur in userRoles
                join rp in rolePermissions on ur.RoleId equals rp.RoleId
                join p in permissions on rp.PermissionId equals p.Id
                where ur.UserId == userId && !ur.IsDeleted && !rp.IsDeleted && !p.IsDeleted
                select p.Name).Distinct().ToList();
    }
}