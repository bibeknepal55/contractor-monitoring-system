namespace ContractorMonitoring.Application.Common.Models;

// Pagination, filtering, and sorting parameters
public class PaginationFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public Dictionary<string, string>? Filters { get; set; }

    public int Skip => (Page - 1) * PageSize;
}