using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Lab test and quality monitoring entity
public class LabTest : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string TestCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Material, Soil, Concrete, Steel, etc.
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public string? TechnicianName { get; set; }
    public string TestStandard { get; set; } = string.Empty; // ASTM, IS, BS, etc.
    public string? TestResult { get; set; } // Pass, Fail, Conditional
    public string? Remarks { get; set; }
    public string? TestReportPath { get; set; } // File path
    public DateTime? NextTestDate { get; set; }
    public string? ParameterTested { get; set; }
    public string? SpecificationLimit { get; set; }
    public string? ActualValue { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}