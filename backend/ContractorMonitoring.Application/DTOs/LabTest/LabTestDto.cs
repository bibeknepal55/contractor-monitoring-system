namespace ContractorMonitoring.Application.DTOs.LabTest;

public class LabTestDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string TestCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public string? TechnicianName { get; set; }
    public string TestStandard { get; set; } = string.Empty;
    public string? TestResult { get; set; }
    public string? TestReportPath { get; set; }
    public string? ParameterTested { get; set; }
    public string? SpecificationLimit { get; set; }
    public string? ActualValue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLabTestDto
{
    public Guid ProjectId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string TestCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public string? TechnicianName { get; set; }
    public string TestStandard { get; set; } = string.Empty;
    public string? ParameterTested { get; set; }
    public string? SpecificationLimit { get; set; }
}

public class UpdateLabTestDto
{
    public Guid ProjectId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string TestCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string? LabName { get; set; }
    public string? TechnicianName { get; set; }
    public string TestStandard { get; set; } = string.Empty;
    public string? ParameterTested { get; set; }
    public string? SpecificationLimit { get; set; }
    public string? TestResult { get; set; }
    public string? ActualValue { get; set; }
    public string? TestReportPath { get; set; }
    public string? Remarks { get; set; }
}