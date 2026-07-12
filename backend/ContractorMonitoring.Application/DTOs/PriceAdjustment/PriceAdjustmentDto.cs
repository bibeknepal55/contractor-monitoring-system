namespace ContractorMonitoring.Application.DTOs.PriceAdjustment;

public class PriceAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal? PreviousAmount { get; set; }
    public decimal NewAmount { get; set; }
    public decimal? PercentageChange { get; set; }
    public string Currency { get; set; } = "NPR";
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public bool IsApproved { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePriceAdjustmentDto
{
    public Guid ProjectId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal? PreviousAmount { get; set; }
    public decimal NewAmount { get; set; }
    public decimal? PercentageChange { get; set; }
    public string Currency { get; set; } = "NPR";
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public bool IsApproved { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Remarks { get; set; }
}

public class UpdatePriceAdjustmentDto
{
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal? PreviousAmount { get; set; }
    public decimal NewAmount { get; set; }
    public decimal? PercentageChange { get; set; }
    public string Currency { get; set; } = "NPR";
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public bool IsApproved { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Remarks { get; set; }
}