using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Raw material monitoring entity
public class RawMaterial : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Cement, Steel, Aggregate, Sand, etc.
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityUsed { get; set; }
    public decimal QuantityInStock => QuantityReceived - QuantityUsed;
    public string Unit { get; set; } = string.Empty; // kg, ton, m3, etc.
    public decimal UnitPrice { get; set; }
    public decimal TotalCost => QuantityOrdered * UnitPrice;
    public string? SupplierName { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? QualityCertificate { get; set; }
    public string Status { get; set; } = string.Empty; // Ordered, InTransit, Received, InUse, Depleted

    // Navigation properties
    public Project Project { get; set; } = null!;
}