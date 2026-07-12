namespace ContractorMonitoring.Application.DTOs.DelayReason;

public class DelayReasonDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string DelayCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DelayStartDate { get; set; }
    public DateTime? DelayEndDate { get; set; }
    public int DelayDays { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string? ResponsibleParty { get; set; }
    public string? MitigationAction { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDelayReasonDto
{
    public Guid ProjectId { get; set; }
    public string DelayCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DelayStartDate { get; set; }
    public DateTime? DelayEndDate { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string? ResponsibleParty { get; set; }
    public string? MitigationAction { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateDelayReasonDto
{
    public DateTime? DelayEndDate { get; set; }
    public int DelayDays { get; set; }
    public string ImpactLevel { get; set; } = string.Empty;
    public string? MitigationAction { get; set; }
    public string? Remarks { get; set; }
}