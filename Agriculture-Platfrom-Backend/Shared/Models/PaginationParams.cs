namespace AgriculturalMonitorSystem.Shared.Models;

public class PaginationParams
{
    private int _pageSize = 20;
    private const int MaxPageSize = 100;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}
