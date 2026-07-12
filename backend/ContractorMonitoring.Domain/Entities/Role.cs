using ContractorMonitoring.Domain.Entities.Base;
using ContractorMonitoring.Domain.Enums;

namespace ContractorMonitoring.Domain.Entities;

// Role entity for role-based access control
public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}