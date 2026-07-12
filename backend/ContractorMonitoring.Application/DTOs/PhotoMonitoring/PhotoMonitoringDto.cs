namespace ContractorMonitoring.Application.DTOs.PhotoMonitoring;

public class PhotoMonitoringDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime PhotoDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Direction { get; set; }
    public string? PhotoType { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePhotoMonitoringDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime PhotoDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Direction { get; set; }
    public string? PhotoType { get; set; }
    public string? Tags { get; set; }
}

public class UpdatePhotoMonitoringDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Direction { get; set; }
    public string? Tags { get; set; }
}

// DTO for file upload
public class UploadPhotoDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PhotoDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Direction { get; set; }
    public string? PhotoType { get; set; }
    public string? Tags { get; set; }
}

// Photo download response
public class PhotoDownloadDto
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}