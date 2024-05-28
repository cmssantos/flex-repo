using System.Linq.Expressions;
using FlexRepo.Extensions;
using FlexRepo.Interfaces;
using FlexRepo.Models;
using Microsoft.EntityFrameworkCore;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]

namespace FlexRepo.Repositories;

// <summary>
/// Generic repository implementation.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
public class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T, TKey}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    [CLSCompliant(false)]
    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "DbContext cannot be null");
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        => await GetAllAsync(null, null, "", cancellationToken);

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync(string includeProperties, CancellationToken cancellationToken)
        => await GetAllAsync(null, null, includeProperties, cancellationToken);

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IOrderedQueryable<T>> orderByExpression,
        string includeProperties,
        CancellationToken cancellationToken)
            => await GetAllAsync(null, orderByExpression, includeProperties, cancellationToken);

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filterExpression,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression,
        string includeProperties,
        CancellationToken cancellationToken)
    {
        IQueryable<T> query = _dbSet;

        if (filterExpression != null)
        {
            query = query.Where(filterExpression);
        }

        query = query.ApplyIncludes(includeProperties);

        if (orderByExpression != null)
        {
            query = orderByExpression(query);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaginatedList<T>> GetPaginatedAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
            => await GetPaginatedAsync(pageIndex, pageSize, null, null, "", cancellationToken);

    /// <inheritdoc/>
    public async Task<PaginatedList<T>> GetPaginatedAsync(
        int pageIndex,
        int pageSize,
        string includeProperties,
        CancellationToken cancellationToken)
            => await GetPaginatedAsync(pageIndex, pageSize, null, null, includeProperties, cancellationToken);

    /// <inheritdoc/>
    public async Task<PaginatedList<T>> GetPaginatedAsync(
        int pageIndex,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression,
        string includeProperties,
        CancellationToken cancellationToken)
            => await GetPaginatedAsync(
                pageIndex,
                pageSize,
                null,
                orderByExpression,
                includeProperties,
                cancellationToken);

    /// <inheritdoc/>
    public async Task<PaginatedList<T>> GetPaginatedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? filterExpression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderByExpression = null,
        string includeProperties = "",
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (filterExpression != null)
        {
            query = query.Where(filterExpression);
        }

        query = query.ApplyIncludes(includeProperties);

        if (orderByExpression != null)
        {
            query = orderByExpression(query);
        }

        return await PaginatedList<T>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
    }

    /// <inheritdoc/>
    public T? GetById(TKey id) => GetById(id, "");

    /// <inheritdoc/>
    public T? GetById(TKey id, string includeProperties)
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return query.FirstOrDefault(e => EF.Property<TKey>(e, GetPrimaryKeyName())!.Equals(id));
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(TKey id)
        => await GetByIdAsync(id, "", CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
        => await GetByIdAsync(id, "", cancellationToken);

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(TKey id, string includeProperties)
        => await GetByIdAsync(id, includeProperties, CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(
        TKey id,
        string includeProperties,
        CancellationToken cancellationToken)
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return await query.FirstOrDefaultAsync(e =>
            EF.Property<TKey>(e, GetPrimaryKeyName())!.Equals(id), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await GetSingleOrDefaultAsync(predicate, "", CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken)
        => await GetSingleOrDefaultAsync(predicate, "", cancellationToken);

    /// <inheritdoc/>
    public async Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string includeProperties)
        => await GetSingleOrDefaultAsync(predicate, includeProperties, CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T?> GetSingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string includeProperties,
        CancellationToken cancellationToken)
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public T Add(T entity) => Add(entity, false);

    /// <inheritdoc/>
    public T Add(T entity, bool saveChanges)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Add(entity);

        if (saveChanges)
        {
            _context.SaveChanges();
        }

        return entity;
    }

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity) => await AddAsync(entity, false, CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
        => await AddAsync(entity, false, cancellationToken);

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity, bool saveChanges)
        => await AddAsync(entity, saveChanges, CancellationToken.None);

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity, bool saveChanges, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbSet.AddAsync(entity, cancellationToken);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    /// <inheritdoc/>
    public void Update(T entity) => Update(entity, false);

    /// <inheritdoc/>
    public void Update(T entity, bool saveChanges)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        if (saveChanges)
        {
            _context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(T entity) => await UpdateAsync(entity, false, CancellationToken.None);

    /// <inheritdoc/>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
        => await UpdateAsync(entity, false, cancellationToken);

    /// <inheritdoc/>
    public async Task UpdateAsync(T entity, bool saveChanges)
        => await UpdateAsync(entity, saveChanges, CancellationToken.None);

    /// <inheritdoc/>
    public async Task UpdateAsync(T entity, bool saveChanges, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public void Delete(TKey id) => Delete(id, false);

    /// <inheritdoc/>
    public void Delete(TKey id, bool saveChanges)
    {
        var entity = GetById(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");

        _dbSet.Remove(entity);

        if (saveChanges)
        {
            _context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(TKey id) => await DeleteAsync(id, false, CancellationToken.None);

    /// <inheritdoc/>
    public async Task DeleteAsync(TKey id, CancellationToken cancellationToken)
        => await DeleteAsync(id, false, cancellationToken);

    /// <inheritdoc/>
    public async Task DeleteAsync(TKey id, bool saveChanges)
        => await DeleteAsync(id, saveChanges, CancellationToken.None);

    /// <inheritdoc/>
    public async Task DeleteAsync(TKey id, bool saveChanges, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(id, cancellationToken: cancellationToken) ??
            throw new KeyNotFoundException($"Entity with id {id} not found.");

        _dbSet.Remove(entity);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public int SaveChanges() => _context.SaveChanges();

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
        => await SaveChangesAsync(CancellationToken.None);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);

    private string GetPrimaryKeyName()
    {
        var entityType = _context.Model.FindEntityType(typeof(T));

        return entityType?.FindPrimaryKey()?.Properties.Select(x => x.Name).FirstOrDefault() ??
            throw new InvalidOperationException("Primary key not found for entity type.");
    }
}
