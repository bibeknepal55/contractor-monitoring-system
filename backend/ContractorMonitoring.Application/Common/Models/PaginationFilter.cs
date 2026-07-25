namespace ContractorMonitoring.Application.Common.Models;

public class PaginationFilter
{
    private int _pageSize = 10;
    private int _page = 1;

    public const int MaxPageSize = 100;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public Dictionary<string, string>? Filters { get; set; }

    public int Skip => (Page - 1) * PageSize;
}
