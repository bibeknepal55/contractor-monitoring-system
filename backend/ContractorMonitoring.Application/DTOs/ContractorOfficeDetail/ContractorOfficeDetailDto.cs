namespace ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

// Contractor office detail response DTO
public class ContractorOfficeDetailDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public string? ContactPersonEmail { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? InsuranceDetails { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public DateTime CreatedAt { get; set; }
}