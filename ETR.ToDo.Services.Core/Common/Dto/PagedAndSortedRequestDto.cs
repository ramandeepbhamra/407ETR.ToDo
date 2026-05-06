namespace ETR.ToDo.Services.Core.Common.Dto;

/// <summary>
/// Base input for paginated and sorted requests
/// </summary>
public class PagedAndSortedRequestDto
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field name
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Search keyword
    /// </summary>
    public string? SearchKeyword { get; set; }
}
