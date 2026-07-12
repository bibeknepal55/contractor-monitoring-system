using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Performance bond management entity
public class PerformanceBond : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string BondNumber { get; set; } = string.Empty;
    public decimal BondAmount { get; set; }
    public string BondType { get; set; } = string.Empty; // Bank Guarantee, Insurance Bond, Cash
    public string IssuingBank { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? BondDocument { get; set; } // File path
    public string Status { get; set; } = string.Empty; // Active, Expired, Released, Forfeited
    public string? Remarks { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}