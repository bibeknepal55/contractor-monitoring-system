namespace ContractorMonitoring.Application.DTOs.ResponsibleOfficial;

public class ResponsibleOfficialDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? AppointmentDate { get; set; }
    public DateTime? RelievingDate { get; set; }
    public bool IsActive { get; set; }
    public string? Qualifications { get; set; }
    public int? YearsOfExperience { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateResponsibleOfficialDto
{
    public Guid ProjectId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? AppointmentDate { get; set; }
    public string? Qualifications { get; set; }
    public int? YearsOfExperience { get; set; }
}

public class UpdateResponsibleOfficialDto
{
    public Guid ProjectId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? AppointmentDate { get; set; }
    public string? Qualifications { get; set; }
    public int? YearsOfExperience { get; set; }
    public DateTime? RelievingDate { get; set; }
    public bool IsActive { get; set; }
}