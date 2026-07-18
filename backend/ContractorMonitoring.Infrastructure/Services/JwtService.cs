using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Infrastructure.Services;

// JWT token generation and validation service
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public JwtService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]!);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Get user roles and permissions from centralized methods
        var roles = await GetUserRolesAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("TenantId", user.TenantId.ToString()),
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permission claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return await Task.FromResult(Convert.ToBase64String(randomNumber));
    }

    public async Task<(bool isValid, Guid userId)> ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            return await Task.FromResult((true, userId));
        }
        catch
        {
            return await Task.FromResult((false, Guid.Empty));
        }
    }

    public async Task<DateTime> GetTokenExpiryTime(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        return await Task.FromResult(jwtToken.ValidTo);
    }

    // Public - Centralized role resolution for reuse by Login/Register handlers
    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();

        return (from ur in userRoles
                join r in roles on ur.RoleId equals r.Id
                where ur.UserId == userId && !ur.IsDeleted && !r.IsDeleted
                select r.Name).ToList();
    }

    // Public - Centralized permission resolution for reuse by Login/Register handlers
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        var roles = await _unitOfWork.Roles.GetAllAsync();

        // Check if user is SuperAdmin - bypass permission checks
        var userRoleList = (from ur in userRoles
                            join r in roles on ur.RoleId equals r.Id
                            where ur.UserId == userId && !ur.IsDeleted && !r.IsDeleted
                            select r.Name).ToList();

        if (userRoleList.Contains("SuperAdmin"))
        {
            return permissions.Where(p => !p.IsDeleted).Select(p => p.Name).ToList();
        }

        return (from ur in userRoles
                join rp in rolePermissions on ur.RoleId equals rp.RoleId
                join p in permissions on rp.PermissionId equals p.Id
                where ur.UserId == userId
                    && !ur.IsDeleted
                    && !rp.IsDeleted
                    && !p.IsDeleted
                select p.Name).Distinct().ToList();
    }
}