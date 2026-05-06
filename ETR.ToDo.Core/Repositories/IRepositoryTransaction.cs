namespace ETR.ToDo.Core.Repositories;

/// <summary>
/// Represents a database transaction
/// Abstraction over infrastructure-specific transaction implementations
/// </summary>
public interface IRepositoryTransaction : IAsyncDisposable
{
    /// <summary>
    /// Commits all changes made in the transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all changes made in the transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
