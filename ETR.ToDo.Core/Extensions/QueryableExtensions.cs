using System.Linq.Expressions;

namespace ETR.ToDo.Core.Extensions;

/// <summary>
/// Query extension methods for building dynamic, composable LINQ queries
/// Provides conditional operators and pagination helpers
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Conditionally applies a Where clause if the condition is true
    /// Useful for building dynamic queries without nested if statements
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to filter</param>
    /// <param name="condition">If true, applies the predicate; otherwise returns query unchanged</param>
    /// <param name="predicate">The filter expression to apply</param>
    /// <returns>Filtered queryable if condition is true, otherwise original queryable</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Applies pagination to a query (Skip + Take)
    /// Useful for implementing paged lists in APIs
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="skipCount">Number of records to skip</param>
    /// <param name="maxResultCount">Maximum number of records to take</param>
    /// <returns>Paginated queryable</returns>
    public static IQueryable<T> PageBy<T>(
        this IQueryable<T> query,
        int skipCount,
        int maxResultCount)
    {
        return query.Skip(skipCount).Take(maxResultCount);
    }

    /// <summary>
    /// Applies pagination using page number and page size
    /// Page numbers are 1-based (first page is 1, not 0)
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated queryable</returns>
    public static IQueryable<T> PageByPageNumber<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var skipCount = (pageNumber - 1) * pageSize;
        return query.Skip(skipCount).Take(pageSize);
    }

    /// <summary>
    /// Conditionally orders a query by a selector if condition is true
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type for ordering</typeparam>
    /// <param name="query">The queryable to order</param>
    /// <param name="condition">If true, applies the ordering; otherwise returns query unchanged</param>
    /// <param name="keySelector">Property selector for ordering</param>
    /// <returns>Ordered queryable if condition is true, otherwise original queryable</returns>
    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
    {
        return condition ? query.OrderBy(keySelector) : query;
    }

    /// <summary>
    /// Conditionally orders a query descending by a selector if condition is true
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type for ordering</typeparam>
    /// <param name="query">The queryable to order</param>
    /// <param name="condition">If true, applies the ordering; otherwise returns query unchanged</param>
    /// <param name="keySelector">Property selector for ordering</param>
    /// <returns>Ordered queryable if condition is true, otherwise original queryable</returns>
    public static IQueryable<T> OrderByDescendingIf<T, TKey>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
    {
        return condition ? query.OrderByDescending(keySelector) : query;
    }

    /// <summary>
    /// Conditionally applies Take (limits results) if condition is true and maxCount is greater than 0
    /// Useful for optional result limiting
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to limit</param>
    /// <param name="condition">If true and maxCount > 0, applies Take; otherwise returns query unchanged</param>
    /// <param name="maxCount">Maximum number of items to take</param>
    /// <returns>Limited queryable if condition is true, otherwise original queryable</returns>
    public static IQueryable<T> TakeIf<T>(
        this IQueryable<T> query,
        bool condition,
        int maxCount)
    {
        return condition && maxCount > 0 ? query.Take(maxCount) : query;
    }

    /// <summary>
    /// Checks if a string is null, empty, or whitespace
    /// Convenience method for cleaner null checks in queries
    /// </summary>
    /// <param name="str">String to check</param>
    /// <returns>True if string is null, empty, or whitespace</returns>
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// Checks if a string has a value (not null, empty, or whitespace)
    /// Convenience method for cleaner null checks in queries
    /// </summary>
    /// <param name="str">String to check</param>
    /// <returns>True if string has a value</returns>
    public static bool HasValue(this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }
}
