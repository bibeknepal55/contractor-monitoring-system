using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Delay reason management entity
public class DelayReason : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string DelayCategory { get; set; } = string.Empty; // Weather, Material, Labor, Design, Approval, Other
    public string Description { get; set; } = string.Empty;
    public DateTime DelayStartDate { get; set; }
    public DateTime? DelayEndDate { get; set; }
    public int DelayDays { get; set; }
    public string ImpactLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string? ResponsibleParty { get; set; } // Contractor, Client, Consultant, External
    public string? MitigationAction { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
