using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Extensions;

/// <summary>
/// Extensions for IQueryable queries.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Applies property includes to an IQueryable query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The IQueryable query.</param>
    /// <param name="includeProperties">The properties to include.</param>
    /// <returns>The IQueryable query with the includes applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the query is null.</exception>
    public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> queryable, string includeProperties) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

        if (string.IsNullOrWhiteSpace(includeProperties))
        {
            return queryable;
        }

        var separator = new[] { ',' };

        foreach (var includeProperty in includeProperties.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            queryable = queryable.Include(includeProperty.Trim());
        }

        return queryable;
    }
}
