namespace ContractorMonitoring.Application.DTOs.RawMaterial;

public class RawMaterialDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityUsed { get; set; }
    public decimal QuantityInStock { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateRawMaterialDto
{
    public Guid ProjectId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? OrderDate { get; set; }
}

public class UpdateRawMaterialDto
{
    public decimal QuantityReceived { get; set; }
    public decimal QuantityUsed { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
}