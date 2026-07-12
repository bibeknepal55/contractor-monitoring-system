using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Subcontractor management entity
public class Subcontractor : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ScopeOfWork { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public decimal ContractAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Completed, Terminated, OnHold
    public string? PerformanceRating { get; set; } // 1-5 rating
    public string? Remarks { get; set; }
    public string? LicenseNumber { get; set; }
    public string? InsuranceDetails { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}