using System.Linq.Expressions;
using FlexRepo.Models;

namespace FlexRepo.Interfaces;

/// <summary>
/// Interface for generic repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public interface IRepository<T, TKey> where T : class
{
    /// <summary>
    /// Retrieves all entities asynchronously.
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="orderByExpression">The order by expression.</param>
    /// <param name="includeProperties">The include properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filterExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        string includeProperties = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities paginated asynchronously.
    /// </summary>
    /// <param name="pageIndex">The index of the page to retrieve.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="filterExpression">The filter expression.</param>
    /// <param name="orderByExpression">The order by expression.</param>
    /// <param name="includeProperties">The include properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the paginated list of entities.</returns>
    Task<PaginatedList<T>> GetPaginatedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? filterExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        string includeProperties = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="includeProperties">The include properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the entity.</returns>
    Task<T?> GetByIdAsync(TKey id, string includeProperties = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single entity asynchronously based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the entity.</param>
    /// <param name="includeProperties">The include properties.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the entity.</returns>
    Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, string includeProperties = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    /// <returns>The added entity.</returns>
    T Add(T entity, bool saveChanges = false);

    /// <summary>
    /// Adds a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the added entity.</returns>
    Task<T> AddAsync(T entity, bool saveChanges = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    /// 
    void Update(T entity, bool saveChanges = false);

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(T entity, bool saveChanges = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    void Delete(TKey id, bool saveChanges = false);

    /// <summary>
    /// Deletes an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="saveChanges">Indicates whether to save the changes immediately.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task DeleteAsync(TKey id, bool saveChanges = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the underlying database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    int SaveChanges();

    /// <summary>
    /// Saves all changes made in this context to the underlying database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the asynchronous operation, returning the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
