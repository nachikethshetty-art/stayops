namespace StayOps.Application.Common.Models;

public class PagedRequest
{
    private int _pageSize = 20;
    public int Page { get; set; } = 1;
    public int PageSize { get => _pageSize; set => _pageSize = value is > 0 and <= 200 ? value : _pageSize; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? Search { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
