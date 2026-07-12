namespace ContractorMonitoring.Application.DTOs.ContractorOfficeDetail;

// Update contractor office detail request DTO
public class UpdateContractorOfficeDetailDto
{
    public string CompanyName { get; set; } = string.Empty;
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
}