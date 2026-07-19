using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Audit trail for permission changes - who changed what, when
public class RolePermissionHistory : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public string Action { get; set; } = string.Empty; // "Added" or "Removed"
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}