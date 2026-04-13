/**
 * PagedResult - Generic Pagination Response Model
 *
 * Provides a standardized structure for paginated API responses.
 * Includes metadata about pagination state and navigation.
 *
 * Features:
 * - Generic type support for any entity
 * - Total count and page calculations
 * - Navigation metadata (hasNext, hasPrevious)
 * - Page size and number tracking
 */

namespace ThesisBackend.Domain.Common;

/// <summary>
/// Represents a paginated result set with metadata
/// </summary>
/// <typeparam name="T">The type of items in the result set</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// The number of items in the current page
    /// </summary>
    public int CurrentPageSize => Items.Count();

    /// <summary>
    /// Whether this is the first page
    /// </summary>
    public bool IsFirstPage => PageNumber == 1;

    /// <summary>
    /// Whether this is the last page
    /// </summary>
    public bool IsLastPage => PageNumber == TotalPages;

    /// <summary>
    /// Creates a new paged result
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Creates a new paged result with data
    /// </summary>
    /// <param name="items">The items in the current page</param>
    /// <param name="totalCount">The total number of items</param>
    /// <param name="pageNumber">The current page number</param>
    /// <param name="pageSize">The page size</param>
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Maps the items to a different type
    /// </summary>
    /// <typeparam name="TResult">The target type</typeparam>
    /// <param name="mapper">The mapping function</param>
    /// <returns>A paged result with mapped items</returns>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        return new PagedResult<TResult>
        {
            Items = Items.Select(mapper),
            PageNumber = PageNumber,
            PageSize = PageSize,
            TotalCount = TotalCount
        };
    }
}

/// <summary>
/// Pagination query parameters
/// </summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    /// <summary>
    /// The page number to retrieve (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Max(1, Math.Min(value, MaxPageSize));
    }

    /// <summary>
    /// Optional sort field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Whether to sort in descending order
    /// </summary>
    public bool IsDescending => SortDirection?.ToLower() == "desc";
}
