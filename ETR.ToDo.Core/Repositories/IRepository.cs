using ETR.ToDo.Core.Entities.Base;
using System.Linq.Expressions;

namespace ETR.ToDo.Core.Repositories;

/// <summary>
/// Generic repository interface for basic CRUD operations with explicit key type
/// Returns IQueryable for flexible query composition in service layer
/// Developers must explicitly specify both TEntity and TKey type parameters
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type (int, long, Guid, string, etc.) - must be explicitly declared</typeparam>
public interface IRepository<TEntity, TKey> 
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    // Query Operations (Returns IQueryable)

    /// <summary>
    /// Returns IQueryable for flexible query composition in service layer
    /// Use this for building complex queries with LINQ
    /// </summary>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Returns IQueryable with eager loading of related entities
    /// Use this to avoid N+1 query problems
    /// </summary>
    IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);

    // Convenience Methods

    /// <summary>
    /// Gets all entities as a list (executes query immediately)
    /// </summary>
    Task<List<TEntity>> GetAllListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets filtered entities as a list (executes query immediately)
    /// </summary>
    Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets first entity matching predicate or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Command Operations
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    // Utilities
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Transaction Support

    /// <summary>
    /// Begins a database transaction for atomic operations
    /// Use this when multiple operations need to succeed or fail together
    /// </summary>
    Task<IRepositoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
