using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Responsible officials entity
public class ResponsibleOfficial : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty; // Contractor, Client, Consultant
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Role { get; set; } = string.Empty; // ProjectManager, SiteEngineer, Supervisor, Inspector
    public DateTime? AppointmentDate { get; set; }
    public DateTime? RelievingDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public string? Qualifications { get; set; }
    public int? YearsOfExperience { get; set; }

    // Navigation properties
    public Project? Project { get; set; }
}