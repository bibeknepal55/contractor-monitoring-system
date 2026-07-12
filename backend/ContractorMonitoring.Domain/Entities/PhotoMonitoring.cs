using ContractorMonitoring.Domain.Entities.Base;

namespace ContractorMonitoring.Domain.Entities;

// Photo monitoring entity for visual progress tracking
public class PhotoMonitoring : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime PhotoDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Direction { get; set; } // North, South, East, West
    public string? PhotoType { get; set; } // Before, During, After, Defect, Progress
    public string? Tags { get; set; } // Comma-separated tags
    public string? UploadedBy { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}