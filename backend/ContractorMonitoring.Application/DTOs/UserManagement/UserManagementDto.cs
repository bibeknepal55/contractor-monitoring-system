namespace ContractorMonitoring.Application.DTOs.UserManagement;

// User management response DTO
public class UserManagementDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Update user roles request DTO
public class UpdateUserRolesDto
{
    public List<string> Roles { get; set; } = new();
}

// Update user status request DTO
public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

// Assign role request DTO
public class AssignRoleDto
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

// Remove role request DTO
public class RemoveRoleDto
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

// Role management response DTO
public class RoleManagementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public int UserCount { get; set; }
}

// Role permissions request DTO
public class UpdateRolePermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}