namespace ContractorMonitoring.Application.DTOs.RoleManagement;

// Role list item
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Create custom role
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
}

// Update role
public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> PermissionIds { get; set; } = new();
}

// Permission tree for role dialog
public class ModulePermissionDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;
    public List<PermissionItemDto> Permissions { get; set; } = new();
}

public class PermissionItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Create, View, Update, Delete
}