using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Phase 2: Multi-tenancy — each tenant is an organisation
public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;   // e.g. "pwdnepal" → pwdnepal.cms.gov
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; } = "#1a73e8";
    public string? SecondaryColor { get; set; } = "#0d47a1";
    public string? ConnectionString { get; set; }            // null = shared DB
    public bool IsActive { get; set; } = true;
    public string? AdminEmail { get; set; }
    public string? Plan { get; set; } = "Standard";          // Standard, Enterprise
    public DateTime? TrialEndsAt { get; set; }
    public string? IpAllowlist { get; set; }                 // CSV of allowed IPs/CIDRs
    public bool GeoBlockEnabled { get; set; } = false;
    public string? AllowedCountries { get; set; }            // CSV ISO-3166 codes
}
