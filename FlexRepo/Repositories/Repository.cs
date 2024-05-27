using System.Linq.Expressions;
using FlexRepo.Extensions;
using FlexRepo.Interfaces;
using FlexRepo.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexRepo.Repositories;

// <summary>
/// Generic repository implementation.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
public class Repository<T, TKey, TContext> : IRepository<T, TKey> where T : class where TContext : DbContext
{
    private readonly TContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T, TKey, TContext}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync(
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

        return await query.ToListAsync(cancellationToken);
    }

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
    public T? GetById(TKey id, string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return query.FirstOrDefault(e => EF.Property<TKey>(e, GetPrimaryKeyName())!.Equals(id));
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(TKey id, string includeProperties = "", CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, GetPrimaryKeyName())!.Equals(id), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, string includeProperties = "",
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        query = query.ApplyIncludes(includeProperties);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public T Add(T entity, bool saveChanges = false)
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
    public async Task<T> AddAsync(T entity, bool saveChanges = false, CancellationToken cancellationToken = default)
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
    public void Update(T entity, bool saveChanges = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        if (saveChanges)
        {
            _context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(T entity, bool saveChanges = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public void Delete(TKey id, bool saveChanges = false)
    {
        var entity = GetById(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");

        _dbSet.Remove(entity);

        if (saveChanges)
        {
            _context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(TKey id, bool saveChanges = false, CancellationToken cancellationToken = default)
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
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    private string GetPrimaryKeyName()
    {
        var entityType = _context.Model.FindEntityType(typeof(T));

        return entityType?.FindPrimaryKey()?.Properties.Select(x => x.Name).FirstOrDefault() ??
            throw new InvalidOperationException("Primary key not found for entity type.");
    }
}
