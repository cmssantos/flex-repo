using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Models;

// <summary>
/// Represents a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Gets the index of the current page.
    /// </summary>
    public int PageIndex { get; private set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public List<T> Items { get; private set; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    private PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PaginatedList{T}"/> asynchronously.
    /// </summary>
    /// <param name="source">The source queryable to paginate.</param>
    /// <param name="pageIndex">The index of the page to retrieve.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, returning the paginated list.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageIndex < 1)
        {
            throw new ArgumentException("Page index must be greater than or equal to 1.", nameof(pageIndex));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than or equal to 1.", nameof(pageSize));
        }

        var count = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}