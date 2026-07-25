using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Phase 3: ABAC — attribute-based access control policy
public class ResourcePolicy : AuditableEntity
{
    public string Resource { get; set; } = string.Empty;     // "Project", "ContractorOfficeDetail"
    public string Action { get; set; } = string.Empty;       // "Edit", "View", "Delete"
    public string Attribute { get; set; } = string.Empty;    // "Department", "OwnedBy"
    public string Operator { get; set; } = string.Empty;     // "Equals", "In", "StartsWith"
    public string Value { get; set; } = string.Empty;        // "PWD", "user.department"
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}

// Phase 3: Time-bound role assignment
public class TimeBoundUserRole : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string? Reason { get; set; }
    public bool IsActive => DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo && !IsDeleted;
    public User? User { get; set; }
    public Role? Role { get; set; }
}

// Phase 3: Permission inheritance — child role inherits from parent
public class RoleInheritance : AuditableEntity
{
    public Guid ChildRoleId { get; set; }
    public Guid ParentRoleId { get; set; }
    public Role? ChildRole { get; set; }
    public Role? ParentRole { get; set; }
}
