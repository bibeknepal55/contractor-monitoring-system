using ContractorMonitoring.Domain.Entities.Base;
using static ContractorMonitoring.Domain.Constants.Permissions;

namespace ContractorMonitoring.Domain.Entities;

// Project entity - Core entity for monitoring government contractor projects
public class Project : AuditableEntity
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed, OnHold, Cancelled
    public decimal Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ProjectManager { get; set; }
    public string? ContactNumber { get; set; }
    public string? ContractNumber { get; set; }
    public string Priority { get; set; } = string.Empty; // Low, Medium, High, Critical
    public double? ProgressPercentage { get; set; }

    // Foreign Keys
    public Guid ContractorId { get; set; }

    // Navigation properties
    public ContractorOfficeDetail Contractor { get; set; } = null!;
    public ICollection<ContractFinancialDetail> ContractFinancialDetails { get; set; } = new List<ContractFinancialDetail>();
    public ICollection<PhysicalProgress> PhysicalProgresses { get; set; } = new List<PhysicalProgress>();
    public ICollection<TimeExtension> TimeExtensions { get; set; } = new List<TimeExtension>();
}